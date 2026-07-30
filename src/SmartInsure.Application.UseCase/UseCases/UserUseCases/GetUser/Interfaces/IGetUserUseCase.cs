using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.GetUser.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.GetUser.Responses;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.GetUser.Interfaces;

public interface IGetUserUseCase : IUseCase<GetUserRequest, GetUserResponse>
{
}
