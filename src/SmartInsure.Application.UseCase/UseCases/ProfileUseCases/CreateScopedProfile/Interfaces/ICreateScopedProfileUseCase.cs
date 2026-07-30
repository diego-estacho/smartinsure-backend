using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.CreateScopedProfile.Requests;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.CreateScopedProfile.Responses;

namespace SmartInsure.Application.UseCase.UseCases.ProfileUseCases.CreateScopedProfile.Interfaces;

public interface ICreateScopedProfileUseCase
    : IUseCase<CreateScopedProfileRequest, CreateScopedProfileResponse>;
