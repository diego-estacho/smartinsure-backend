using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.GetProfile.Requests;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.GetProfile.Responses;

namespace SmartInsure.Application.UseCase.UseCases.ProfileUseCases.GetProfile.Interfaces;

public interface IGetProfileUseCase : IUseCase<GetProfileRequest, GetProfileResponse>
{
}
