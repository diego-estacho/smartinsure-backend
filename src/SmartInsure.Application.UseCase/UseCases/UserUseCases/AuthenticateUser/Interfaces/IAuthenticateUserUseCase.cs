using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.AuthenticateUser.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.AuthenticateUser.Responses;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.AuthenticateUser.Interfaces;

/// <summary>Contrato para o caso de uso de autenticação de usuário.</summary>
public interface IAuthenticateUserUseCase : IUseCase<AuthenticateUserRequest, AuthenticateUserResponse>;
