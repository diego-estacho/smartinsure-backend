using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.GetInsurerEnablement.Interfaces;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.GetInsurerEnablement.Requests;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.GetInsurerEnablement.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.GetInsurerEnablement;

/// <summary>RN-022 — detalhe da Habilitação de Seguradora (Inativa permanece consultável).</summary>
public sealed class GetInsurerEnablementUseCase(
    IInsurerEnablementRepository enablementRepository) : IGetInsurerEnablementUseCase
{
    public async Task<GetInsurerEnablementResponse> ExecuteAsync(
        GetInsurerEnablementRequest request,
        CancellationToken cancellationToken)
    {
        var details = await enablementRepository.GetDetailsByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Habilitação não encontrada.");

        return new GetInsurerEnablementResponse(
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
