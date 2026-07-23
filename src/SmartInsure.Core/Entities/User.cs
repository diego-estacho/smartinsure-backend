using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Core.Entities;

/// <summary>
/// Usuário da plataforma (RN-001, RN-002, RN-012): nasce Pendente, sempre com identidade
/// correspondente no provedor de identidade referenciada por <see cref="ExternalIdentity"/>.
/// Perfil é opcional (RN-012).
/// </summary>
public sealed class User : EntityBase
{
    private User()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string ExternalIdentity { get; private set; } = string.Empty;

    public EUserStatus Status { get; private set; }

    /// <summary>RN-012: Perfil de Escopo System do Usuário (ex.: Administrador do Sistema); nulo = usuário comum.</summary>
    public Guid? ProfileId { get; private set; }

    public Profile? Profile { get; private set; }

    public static User Create(string name, string email, string externalIdentity)
    {
        if (string.IsNullOrWhiteSpace(externalIdentity))
        {
            throw new BusinessRuleException(
                "Usuário não pode existir sem identidade no provedor de identidade.");
        }

        return new User
        {
            Name = name.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            ExternalIdentity = externalIdentity,
            Status = EUserStatus.Pending,
        };
    }

    /// <summary>RN-002: ativação ao concluir o primeiro acesso com senha própria definida.</summary>
    public void Activate()
    {
        Status = EUserStatus.Active;
    }

    /// <summary>RN-046: inativa o Usuário Ativo (Usuário Inativo não acessa a plataforma).</summary>
    public void Deactivate()
    {
        if (Status != EUserStatus.Active)
        {
            throw new ConflictException("Somente um usuário ativo pode ser inativado.");
        }

        Status = EUserStatus.Inactive;
    }

    /// <summary>RN-046: reativa o Usuário Inativo (Inativo → Ativo).</summary>
    public void Reactivate()
    {
        if (Status != EUserStatus.Inactive)
        {
            throw new ConflictException("Somente um usuário inativo pode ser reativado.");
        }

        Status = EUserStatus.Active;
    }

    /// <summary>RN-012: concessão do Perfil (conceder o mesmo Perfil de novo é conflito).</summary>
    public void GrantProfile(Profile profile)
    {
        if (ProfileId == profile.Id)
        {
            throw new ConflictException("O usuário já possui este perfil.");
        }

        ProfileId = profile.Id;
        Profile = profile;
    }

    /// <summary>RN-012: revogação do Perfil (revogar de quem não tem é conflito).</summary>
    public void RevokeProfile()
    {
        if (ProfileId is null)
        {
            throw new ConflictException("O usuário não possui perfil a revogar.");
        }

        ProfileId = null;
        Profile = null;
    }
}
