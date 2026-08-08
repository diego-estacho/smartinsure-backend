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

    /// <summary>
    /// RN-082: CPF do Usuário (somente dígitos), identifica a pessoa — imutável. Nulo apenas para
    /// Usuários pré-existentes à RN-082; os fluxos de convite exigem CPF válido (11 dígitos).
    /// </summary>
    public string? DocumentNumber { get; private set; }

    public string ExternalIdentity { get; private set; } = string.Empty;

    public EUserStatus Status { get; private set; }

    /// <summary>RN-204: instante do último acesso concluído (login bem-sucedido, RN-005); nulo = nunca acessou.</summary>
    public DateTime? LastAccessAtUtc { get; private set; }

    /// <summary>RN-012: Perfil de Escopo System do Usuário (ex.: Administrador do Sistema); nulo = usuário comum.</summary>
    public Guid? ProfileId { get; private set; }

    public Profile? Profile { get; private set; }

    public static User Create(string name, string email, string externalIdentity, string? documentNumber = null)
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
            DocumentNumber = NormalizeDocumentNumber(documentNumber),
            ExternalIdentity = externalIdentity,
            Status = EUserStatus.Pending,
        };
    }

    /// <summary>RN-082: guarda o CPF só em dígitos; quando informado, exige exatamente 11.</summary>
    private static string? NormalizeDocumentNumber(string? documentNumber)
    {
        if (string.IsNullOrWhiteSpace(documentNumber))
        {
            return null;
        }

        var digits = new string([.. documentNumber.Where(char.IsDigit)]);

        if (digits.Length != 11)
        {
            throw new BusinessRuleException("O CPF do usuário deve conter 11 dígitos.");
        }

        return digits;
    }

    /// <summary>RN-002: ativação ao concluir o primeiro acesso com senha própria definida.</summary>
    public void Activate()
    {
        Status = EUserStatus.Active;
    }

    /// <summary>RN-204: registra o instante de um acesso concluído (login bem-sucedido, RN-005).</summary>
    public void RecordAccess()
    {
        LastAccessAtUtc = DateTime.UtcNow;
    }

    /// <summary>RN-076: inativa o Usuário Ativo (Usuário Inativo não acessa a plataforma).</summary>
    public void Deactivate()
    {
        if (Status != EUserStatus.Active)
        {
            throw new ConflictException("Somente um usuário ativo pode ser inativado.");
        }

        Status = EUserStatus.Inactive;
    }

    /// <summary>RN-076: reativa o Usuário Inativo (Inativo → Ativo).</summary>
    public void Reactivate()
    {
        if (Status != EUserStatus.Inactive)
        {
            throw new ConflictException("Somente um usuário inativo pode ser reativado.");
        }

        Status = EUserStatus.Active;
    }

    /// <summary>RN-202: corrige o nome de cadastro; o histórico do Usuário é preservado.</summary>
    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BusinessRuleException("O nome do usuário é obrigatório.");
        }

        Name = name.Trim();
    }

    /// <summary>
    /// RN-202: corrige o e-mail SÓ enquanto Pendente — antes do primeiro acesso não há credencial;
    /// depois o e-mail é a credencial de acesso e não se altera por edição.
    /// </summary>
    public void ChangeEmail(string email)
    {
        if (Status != EUserStatus.Pending)
        {
            throw new ConflictException(
                "O e-mail só pode ser corrigido enquanto o usuário está pendente.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new BusinessRuleException("O e-mail do usuário é obrigatório.");
        }

        Email = email.Trim().ToLowerInvariant();
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
