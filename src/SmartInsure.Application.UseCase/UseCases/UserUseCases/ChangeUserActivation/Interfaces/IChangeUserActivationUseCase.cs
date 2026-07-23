using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ChangeUserActivation.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ChangeUserActivation.Responses;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.ChangeUserActivation.Interfaces;

/// <summary>Contrato da inativação/reativação de Usuário (RN-046).</summary>
public interface IChangeUserActivationUseCase
    : IUseCase<ChangeUserActivationRequest, ChangeUserActivationResponse>;
