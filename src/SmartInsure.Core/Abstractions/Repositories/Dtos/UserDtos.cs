namespace SmartInsure.Core.Abstractions.Repositories.Dtos;

public sealed record UserListItemDto(
    Guid Id,
    string Name,
    string Email,
    string Status,
    string? ProfileName,
    DateTime CreatedAt);

public sealed record UserDetailsDto(
    Guid Id,
    string Name,
    string Email,
    string Status,
    Guid? ProfileId,
    string? ProfileName,
    DateTime CreatedAt,
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
    string ProfileName);
