using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Core.Entities;

/// <summary>
/// Perfil (RN-062): conjunto nomeado de Permissões, com um Escopo (Sistema, uma Corretora ou um
/// Tomador). Perfis fixos (ex.: Administrador do Sistema) só têm as Permissões editadas pelo
/// Administrador do Sistema (RN-073); customizados são criados no seu Escopo. Nesta fatia
/// (exec-plan 0008) apenas o Escopo System é exercido; os vínculos de Corretora/Tomador entram na fatia 1.
/// </summary>
public sealed class Profile : EntityBase
{
    private readonly List<ProfilePermission> _permissions = [];

    private Profile()
    {
    }

    public string Name { get; private set; } = string.Empty;

    /// <summary>RN-082: descrição livre e opcional; não participa de autorização nem de unicidade.</summary>
    public string? Description { get; private set; }

    public EProfileScope Scope { get; private set; }

    public bool IsFixed { get; private set; }

    /// <summary>Corretora dona do Perfil quando o Escopo é Brokerage (fatia 1).</summary>
    public Guid? BrokerageId { get; private set; }

    /// <summary>Tomador dono do Perfil quando o Escopo é PolicyHolder (fatia 1).</summary>
    public Guid? PolicyHolderId { get; private set; }

    public IReadOnlyCollection<ProfilePermission> Permissions => _permissions.AsReadOnly();

    public static Profile Create(string name, EProfileScope scope, bool isFixed, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BusinessRuleException("O perfil precisa de um nome.");
        }

        return new Profile
        {
            Name = name.Trim(),
            Description = Normalize(description),
            Scope = scope,
            IsFixed = isFixed,
        };
    }

    /// <summary>
    /// RN-069: Perfil customizado de uma Corretora — nasce vinculado a ela e só vale ali.
    /// </summary>
    public static Profile CreateForBrokerage(string name, Guid brokerageId, string? description = null)
    {
        var profile = Create(name, EProfileScope.Brokerage, isFixed: false, description);
        profile.BrokerageId = brokerageId;

        return profile;
    }

    /// <summary>RN-070: Perfil customizado de um Tomador — nasce vinculado a ele e só vale ali.</summary>
    public static Profile CreateForPolicyHolder(string name, Guid policyHolderId, string? description = null)
    {
        var profile = Create(name, EProfileScope.PolicyHolder, isFixed: false, description);
        profile.PolicyHolderId = policyHolderId;

        return profile;
    }

    /// <summary>
    /// RN-073/RN-074: Perfil fixo não muda nome, Escopo nem estrutura — só as Permissões dele,
    /// e apenas pelo Administrador do Sistema. Renomear é exclusivo do Perfil customizado.
    /// </summary>
    public void Rename(string name)
    {
        if (IsFixed)
        {
            throw new BusinessRuleException("Perfil fixo da plataforma não pode ser renomeado.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BusinessRuleException("O perfil precisa de um nome.");
        }

        Name = name.Trim();
    }

    /// <summary>
    /// RN-082: ajusta a Descrição do Perfil customizado. Perfil fixo não muda por tela (RN-073).
    /// Vazio ou só espaços = sem descrição.
    /// </summary>
    public void SetDescription(string? description)
    {
        if (IsFixed)
        {
            throw new BusinessRuleException("Perfil fixo da plataforma não tem a descrição editada por tela.");
        }

        Description = Normalize(description);
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>
    /// RN-063/RN-073/RN-074: substitui as Permissões marcadas pelo conjunto informado — o que
    /// não está na lista deixa de valer. As Permissões vêm do catálogo fixo da plataforma.
    /// </summary>
    public void ReplacePermissions(IEnumerable<Permission> permissions)
    {
        var target = permissions.Select(permission => permission.Id).Distinct().ToList();

        _permissions.RemoveAll(profilePermission => !target.Contains(profilePermission.PermissionId));

        foreach (var permissionId in target.Where(id => !HasPermission(id)))
        {
            _permissions.Add(ProfilePermission.Create(Id, permissionId));
        }
    }

    /// <summary>RN-062/RN-063: marca uma Permissão no Perfil (idempotente por Permissão).</summary>
    public void AddPermission(Permission permission)
    {
        if (HasPermission(permission.Id))
        {
            return;
        }

        _permissions.Add(ProfilePermission.Create(Id, permission.Id));
    }

    /// <summary>RN-063: o Perfil concede a Permissão quando ela está marcada.</summary>
    public bool HasPermission(Guid permissionId)
    {
        foreach (var profilePermission in _permissions)
        {
            if (profilePermission.PermissionId == permissionId)
            {
                return true;
            }
        }

        return false;
    }
}
