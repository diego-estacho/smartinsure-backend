using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.RequestPolicyIssuance.Requests;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.RequestPolicyIssuance.Responses;

namespace SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.RequestPolicyIssuance.Interfaces;

/// <summary>RN-500/RN-514: solicita a emissão da Apólice da Cotação escolhida e registra o resultado.</summary>
public interface IRequestPolicyIssuanceUseCase
    : IUseCase<RequestPolicyIssuanceRequest, RequestPolicyIssuanceResponse>
{
}
