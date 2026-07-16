using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.SetUserProfile.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.SetUserProfile.Responses;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.SetUserProfile.Interfaces;

/// <summary>Contrato para o caso de uso de concessão/revogação de perfil.</summary>
public interface ISetUserProfileUseCase : IUseCase<SetUserProfileRequest, SetUserProfileResponse>;
