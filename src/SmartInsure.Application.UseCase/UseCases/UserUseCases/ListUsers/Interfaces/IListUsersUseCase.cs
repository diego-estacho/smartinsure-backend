using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ListUsers.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ListUsers.Responses;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.ListUsers.Interfaces;

public interface IListUsersUseCase : IUseCase<ListUsersRequest, ListUsersResponse>
{
}
