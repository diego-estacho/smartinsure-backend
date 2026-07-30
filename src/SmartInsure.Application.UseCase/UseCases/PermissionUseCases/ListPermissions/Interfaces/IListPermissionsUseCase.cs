using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.PermissionUseCases.ListPermissions.Requests;
using SmartInsure.Application.UseCase.UseCases.PermissionUseCases.ListPermissions.Responses;

namespace SmartInsure.Application.UseCase.UseCases.PermissionUseCases.ListPermissions.Interfaces;

public interface IListPermissionsUseCase
    : IUseCase<ListPermissionsRequest, IReadOnlyList<PermissionResponse>>;
