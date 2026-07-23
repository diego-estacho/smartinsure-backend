using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Core.Entities;

/// <summary>
/// Perfil (RN-032): conjunto nomeado de Permissões, com um Escopo (Sistema, uma Corretora ou um
/// Tomador). Perfis fixos (ex.: Administrador do Sistema) só têm as Permissões editadas pelo
/// Administrador do Sistema (RN-043); customizados são criados no seu Escopo. Nesta fatia
/// (exec-plan 0008) apenas o Escopo System é exercido; os vínculos de Corretora/Tomador entram na fatia 1.
/// </summary>
public sealed class Profile : EntityBase
{
    private readonly List<ProfilePermission> _permissions = [];

    private Profile()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public EProfileScope Scope { get; private set; }

    public bool IsFixed { get; private set; }

    /// <summary>Corretora dona do Perfil quando o Escopo é Brokerage (fatia 1).</summary>
    public Guid? BrokerageId { get; private set; }

    /// <summary>Tomador dono do Perfil quando o Escopo é PolicyHolder (fatia 1).</summary>
    public Guid? PolicyHolderId { get; private set; }

    public IReadOnlyCollection<ProfilePermission> Permissions => _permissions.AsReadOnly();

    public static Profile Create(string name, EProfileScope scope, bool isFixed)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BusinessRuleException("O perfil precisa de um nome.");
        }

        return new Profile
        {
            Name = name.Trim(),
            Scope = scope,
            IsFixed = isFixed,
        };
    }

    /// <summary>RN-032/RN-033: marca uma Permissão no Perfil (idempotente por Permissão).</summary>
    public void AddPermission(Permission permission)
    {
        if (HasPermission(permission.Id))
        {
            return;
        }

        _permissions.Add(ProfilePermission.Create(Id, permission.Id));
    }

    /// <summary>RN-033: o Perfil concede a Permissão quando ela está marcada.</summary>
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
