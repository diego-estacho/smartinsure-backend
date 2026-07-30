using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.GetCurrentUserContext.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.GetCurrentUserContext.Responses;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.GetCurrentUserContext.Interfaces;

public interface IGetCurrentUserContextUseCase
    : IUseCase<GetCurrentUserContextRequest, GetCurrentUserContextResponse>
{
}
