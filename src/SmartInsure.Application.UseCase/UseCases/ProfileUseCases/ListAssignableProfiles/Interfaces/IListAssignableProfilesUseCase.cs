using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.ListAssignableProfiles.Requests;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.ListAssignableProfiles.Responses;

namespace SmartInsure.Application.UseCase.UseCases.ProfileUseCases.ListAssignableProfiles.Interfaces;

public interface IListAssignableProfilesUseCase
    : IUseCase<ListAssignableProfilesRequest, IReadOnlyList<AssignableProfileResponse>>
{
}
