using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.ProfileUseCases.DeleteScopedProfile.Requests;

namespace SmartInsure.Application.UseCase.UseCases.ProfileUseCases.DeleteScopedProfile.Interfaces;

public interface IDeleteScopedProfileUseCase : IUseCase<DeleteScopedProfileRequest, Unit>;
