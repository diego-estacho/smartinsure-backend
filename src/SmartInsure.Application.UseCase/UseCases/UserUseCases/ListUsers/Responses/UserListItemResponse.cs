namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.ListUsers.Responses;

public sealed record UserListItemResponse(
    Guid Id,
    string Name,
    string Email,
    string Status,
    string? ProfileName,
    DateTime CreatedAt);
