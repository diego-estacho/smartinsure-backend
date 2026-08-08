using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ChangeUserScopeProfile.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ChangeUserScopeProfile.Responses;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.ChangeUserScopeProfile.Interfaces;

public interface IChangeUserScopeProfileUseCase
    : IUseCase<ChangeUserScopeProfileRequest, ChangeUserScopeProfileResponse>
{
}
