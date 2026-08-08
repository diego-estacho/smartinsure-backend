using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.EditUser.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.EditUser.Responses;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.EditUser.Interfaces;

public interface IEditUserUseCase : IUseCase<EditUserRequest, EditUserResponse>
{
}
