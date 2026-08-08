using SmartInsure.Core.Enumerators;

namespace SmartInsure.Core.Abstractions.Repositories.Dtos;

public sealed record UserListItemDto(
    Guid Id,
    string Name,
    string Email,
    string Status,
    string? ProfileName,
    string? ProfileScope,
    bool ProfileIsFixed,
    string? Link,
    DateTime CreatedAt,
    bool InviteExpired);

public sealed record UserDetailsDto(
    Guid Id,
    string Name,
    string Email,
    string Status,
    Guid? ProfileId,
    string? ProfileName,
    string? ProfileScope,
    bool ProfileIsFixed,
    DateTime CreatedAt,
    DateTime? InvitedAt,
    DateTime? InviteExpiresAt,
    bool InviteExpired,
    IReadOnlyList<UserMembershipDto> BrokerageMemberships,
    IReadOnlyList<UserMembershipDto> PolicyHolderMemberships);

/// <summary>
/// Vínculo do Usuário com uma Corretora ou um Tomador (RN-064): a Corretora/Tomador é uma
/// Person, e o Perfil é o que o Usuário tem naquele Escopo.
/// </summary>
public sealed record UserMembershipDto(
    Guid Id,
    Guid ScopeId,
    string ScopeDocumentNumber,
    string ScopeName,
    Guid ProfileId,
    string ProfileName,
    string ProfileScope,
    bool ProfileIsFixed);

/// <summary>Contagens por situação para as abas da listagem, respeitando escopo e busca (não o filtro de situação).</summary>
public sealed record UserStatusCountsDto(
    long All,
    long Active,
    long PendingNotExpired,
    long Expired,
    long Inactive);

/// <summary>
/// Filtros da listagem de Usuários. A visibilidade por Escopo (RN-064) é resolvida no servidor a
/// partir de quem consulta (`Visible*`); os demais são o recorte da tela (busca + filtros avançados
/// do §4). `Status` é filtro de aba (não entra nas contagens); os demais entram, para as contagens
/// refletirem o recorte corrente.
/// </summary>
public sealed record UserListFilters
{
    public string? Search { get; init; }

    public EUserListStatusFilter? Status { get; init; }

    /// <summary>RN-064: Corretora ativa de quem consulta (só o Administrador do Sistema vê tudo).</summary>
    public Guid? VisibleBrokerageId { get; init; }

    /// <summary>RN-064: Tomador ativo de quem consulta.</summary>
    public Guid? VisiblePolicyHolderId { get; init; }

    /// <summary>Filtro avançado (§4): Usuários que têm este Perfil (de Sistema ou em algum Vínculo).</summary>
    public Guid? ProfileId { get; init; }

    /// <summary>Filtro avançado (§4): Escopo do Perfil (Sistema/Corretora/Tomador).</summary>
    public EProfileScope? Scope { get; init; }

    /// <summary>Filtro avançado (§4): Vínculo — Corretora/Tomador (Person) ao qual o Usuário pertence.</summary>
    public Guid? LinkId { get; init; }

    /// <summary>Filtro avançado (§4): data de cadastro (createdAt) a partir de.</summary>
    public DateTime? RegisteredFrom { get; init; }

    /// <summary>Filtro avançado (§4): data de cadastro (createdAt) até.</summary>
    public DateTime? RegisteredTo { get; init; }
}
