using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.UpdateScopedProfile.Requests;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.UpdateScopedProfile.Responses;

namespace SmartInsure.Application.UseCase.UseCases.ProfileUseCases.UpdateScopedProfile.Interfaces;

public interface IUpdateScopedProfileUseCase
    : IUseCase<UpdateScopedProfileRequest, UpdateScopedProfileResponse>;
