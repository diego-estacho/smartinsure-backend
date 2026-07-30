namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.InvitePolicyHolderUser.Responses;

/// <summary>RN-070: resultado da criação de Usuário no Tomador ativo.</summary>
public sealed record InvitePolicyHolderUserResponse(
    Guid Id,
    string Name,
    string Email,
    string Status,
    Guid PolicyHolderId,
    Guid ProfileId,
    string ProfileName);
