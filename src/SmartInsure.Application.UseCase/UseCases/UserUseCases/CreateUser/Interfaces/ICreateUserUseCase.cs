using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.CreateUser.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.CreateUser.Responses;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.CreateUser.Interfaces;

/// <summary>Contrato para o caso de uso de criação de usuário.</summary>
public interface ICreateUserUseCase : IUseCase<CreateUserRequest, CreateUserResponse>;
