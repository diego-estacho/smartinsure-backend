namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageUser.Responses;

/// <summary>RN-069: resultado da criação de Usuário na Corretora ativa.</summary>
public sealed record InviteBrokerageUserResponse(
    Guid Id,
    string Name,
    string Email,
    string Status,
    Guid BrokerageId,
    Guid ProfileId,
    string ProfileName);
