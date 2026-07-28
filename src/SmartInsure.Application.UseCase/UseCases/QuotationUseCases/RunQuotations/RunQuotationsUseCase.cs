using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.RunQuotations.Interfaces;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.RunQuotations.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.RunQuotations.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Channels;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.RunQuotations;

/// <summary>
/// RN-056/RN-057/RN-060: solicita as Cotações de um Grupo. Resolve o escopo (todas as habilitadas
/// × escolhidas), invalida as Cotações anteriores e a seleção (recálculo), materializa uma Cotação
/// Requested por Seguradora, persiste ANTES de enfileirar (ADR-050) e enfileira o fan-out. O 202 e o
/// acompanhamento por polling ficam na borda (API, ADR-051).
/// </summary>
public sealed class RunQuotationsUseCase(
    IQuotationGroupRepository groupRepository,
    IBrokerageInsurerEnablementRepository enablementRepository,
    IQuotationRepository quotationRepository,
    IQuotationRequestChannel channel,
    IUnitOfWork unitOfWork) : IRunQuotationsUseCase
{
    public async Task<RunQuotationsResponse> ExecuteAsync(
        RunQuotationsRequest request, CancellationToken cancellationToken)
    {
        var group = await groupRepository.GetByIdWithInsurersAsync(request.QuotationGroupId, cancellationToken)
            ?? throw new NotFoundException("Grupo de Cotação não encontrado.");

        var insurerIds = await ResolveScopeAsync(group, request.BrokerageId, cancellationToken);

        // RN-060: recálculo/invalidação — remove as Cotações anteriores e descarta a escolha.
        await quotationRepository.RemoveByGroupAsync(group.Id, cancellationToken);
        group.ClearSelection();
        await unitOfWork.CommitAsync(cancellationToken);

        var quotations = insurerIds
            .Select(insurerId => Quotation.Request(group.Id, request.BrokerageId, insurerId))
            .ToList();

        // ADR-050: o estado é persistido antes do enfileiramento — a fila nunca é o único registro.
        await quotationRepository.AddRangeAsync(quotations, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        foreach (var quotation in quotations)
        {
            await channel.EnqueueAsync(
                new QuotationRequestWorkItem(quotation.Id, group.Id, quotation.InsurerId), cancellationToken);
        }

        return new RunQuotationsResponse(group.Id, quotations.Count);
    }

    private async Task<IReadOnlyList<Guid>> ResolveScopeAsync(
        QuotationGroup group, Guid brokerageId, CancellationToken cancellationToken)
    {
        if (group.ScopeMode == EQuotationScopeMode.Specific)
        {
            var chosen = group.SelectedInsurers.Select(insurer => insurer.InsurerId).Distinct().ToList();

            if (chosen.Count == 0)
            {
                throw new BusinessRuleException("O escopo de Seguradoras escolhidas para cotação está vazio.");
            }

            return chosen;
        }

        // RN-056: escopo All cota todas as Habilitações Ativas da Corretora.
        var active = await enablementRepository.ListActiveByBrokerageAsync(brokerageId, cancellationToken);
        var insurerIds = active.Select(enablement => enablement.InsurerId).Distinct().ToList();

        if (insurerIds.Count == 0)
        {
            throw new BusinessRuleException("A corretora não possui seguradoras habilitadas para cotação.");
        }

        return insurerIds;
    }
}
