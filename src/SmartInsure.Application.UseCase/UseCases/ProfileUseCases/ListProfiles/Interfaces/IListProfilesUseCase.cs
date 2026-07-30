using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.ModelsBase;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.ListProfiles.Requests;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.ListProfiles.Responses;

namespace SmartInsure.Application.UseCase.UseCases.ProfileUseCases.ListProfiles.Interfaces;

public interface IListProfilesUseCase
    : IUseCase<ListProfilesRequest, PagedResponse<ProfileListItemResponse>>
{
}
