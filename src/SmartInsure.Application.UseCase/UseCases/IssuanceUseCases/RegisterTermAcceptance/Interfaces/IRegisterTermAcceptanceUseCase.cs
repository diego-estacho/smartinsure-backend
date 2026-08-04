using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.RegisterTermAcceptance.Requests;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.RegisterTermAcceptance.Responses;

namespace SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.RegisterTermAcceptance.Interfaces;

/// <summary>RN-506: registra o aceite do Termo da Seguradora antes de solicitar a emissão.</summary>
public interface IRegisterTermAcceptanceUseCase
    : IUseCase<RegisterTermAcceptanceRequest, RegisterTermAcceptanceResponse>
{
}
