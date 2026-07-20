using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.GetBrokerageInsurerEnablement.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.GetBrokerageInsurerEnablement.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.GetBrokerageInsurerEnablement.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.GetBrokerageInsurerEnablement;

/// <summary>RN-022 — detalhe da Habilitação de Seguradora (Inativa permanece consultável).</summary>
public sealed class GetBrokerageInsurerEnablementUseCase(
    IBrokerageInsurerEnablementRepository enablementRepository) : IGetBrokerageInsurerEnablementUseCase
{
    public async Task<GetBrokerageInsurerEnablementResponse> ExecuteAsync(
        GetBrokerageInsurerEnablementRequest request,
        CancellationToken cancellationToken)
    {
        var details = await enablementRepository.GetDetailsByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Habilitação não encontrada.");

        return new GetBrokerageInsurerEnablementResponse(
            details.Id,
            details.BrokerageId,
            details.BrokerageName,
            details.InsurerId,
            details.InsurerCorporateName,
            details.CalculationEngine,
            details.ConnectionParameters,
            details.Status);
    }
}
