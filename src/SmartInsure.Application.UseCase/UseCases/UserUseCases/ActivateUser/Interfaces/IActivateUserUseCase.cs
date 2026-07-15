using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ActivateUser.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ActivateUser.Responses;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.ActivateUser.Interfaces;

/// <summary>Contrato para o caso de uso de ativação de usuário.</summary>
public interface IActivateUserUseCase : IUseCase<ActivateUserRequest, ActivateUserResponse>;
