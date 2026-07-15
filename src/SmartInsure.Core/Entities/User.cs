using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Core.Entities;

/// <summary>
/// Usuário da plataforma (RN-001, RN-002): nasce Pendente, sempre com identidade
/// correspondente no provedor de identidade referenciada por <see cref="ExternalIdentity"/>.
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
}
