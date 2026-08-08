namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.GetUser.Responses;

/// <summary>
/// Detalhe do Usuário. `Status` sai pelo nome estável (ADR-031); `InviteExpired` + Pendente é a
/// situação de exibição "Convite expirado" (RN-065). `InvitedAt`/`InviteExpiresAt` vêm do Convite
/// ativo, quando houver.
/// </summary>
public sealed record GetUserResponse(
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
    IReadOnlyList<UserMembershipResponse> BrokerageMemberships,
    IReadOnlyList<UserMembershipResponse> PolicyHolderMemberships);

/// <summary>Vínculo do Usuário com uma Corretora ou um Tomador e o Perfil naquele Escopo (RN-064).</summary>
public sealed record UserMembershipResponse(
    Guid Id,
    Guid ScopeId,
    string ScopeDocumentNumber,
    string ScopeName,
    Guid ProfileId,
    string ProfileName,
    string ProfileScope,
    bool ProfileIsFixed);
