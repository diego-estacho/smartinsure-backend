namespace SmartInsure.Application.UseCase.UseCases.Users.CreateUser;

public sealed record CreateUserRequest(string Name, string Email);

public sealed record CreateUserResponse(Guid Id, string Name, string Email, string Status);
