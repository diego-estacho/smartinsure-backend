using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.SwitchActiveScope.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.SwitchActiveScope.Responses;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.SwitchActiveScope.Interfaces;

public interface ISwitchActiveScopeUseCase
    : IUseCase<SwitchActiveScopeRequest, SwitchActiveScopeResponse>
{
}
