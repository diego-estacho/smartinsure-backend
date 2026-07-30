namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.GetUser.Responses;

public sealed record GetUserResponse(
    Guid Id,
    string Name,
    string Email,
    string Status,
    Guid? ProfileId,
    string? ProfileName,
    DateTime CreatedAt,
    IReadOnlyList<UserMembershipResponse> BrokerageMemberships,
    IReadOnlyList<UserMembershipResponse> PolicyHolderMemberships);

/// <summary>Vínculo do Usuário com uma Corretora ou um Tomador e o Perfil naquele Escopo (RN-064).</summary>
public sealed record UserMembershipResponse(
    Guid Id,
    Guid ScopeId,
    string ScopeDocumentNumber,
    string ScopeName,
    Guid ProfileId,
    string ProfileName);
