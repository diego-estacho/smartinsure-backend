using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.RunQuotations.Interfaces;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.RunQuotations.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.RunQuotations.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.RunQuotations;

/// <summary>
/// RN-056/RN-057 — Solicitação de Cotações (fan-out, request side): resolve o escopo (todas × escolhidas),
/// materializa uma Cotação Requested por Seguradora-alvo (e Indisponível local para as habilitadas não
/// selecionadas, no modo Specific — RN-056), persiste o estado ANTES de enfileirar (ADR-050) e enfileira
/// os itens no Channel; o BackgroundService obtém e persiste cada Cotação incrementalmente (RN-057).
/// RN-060: uma nova solicitação substitui as Cotações anteriores do Grupo e descarta a escolha.
/// </summary>
public sealed class RunQuotationsUseCase(
    IQuotationGroupRepository quotationGroupRepository,
    IBrokerageInsurerEnablementRepository enablementRepository,
    IQuotationRepository quotationRepository,
    IQuotationRequestChannel requestChannel,
    IUnitOfWork unitOfWork) : IRunQuotationsUseCase
{
    public async Task<RunQuotationsResponse> ExecuteAsync(
        RunQuotationsRequest request,
        CancellationToken cancellationToken)
    {
        var group = await quotationGroupRepository.GetByIdWithInsurersAsync(request.QuotationGroupId, cancellationToken)
            ?? throw new NotFoundException("Grupo de Cotação não encontrado.");

        // RN-056: Corretora precisa de ao menos uma Habilitação de Seguradora ativa.
        var activeEnablements = await enablementRepository.ListActiveByBrokerageAsync(
            request.BrokerageId, cancellationToken);

        if (activeEnablements.Count == 0)
        {
            throw new BusinessRuleException("A corretora não possui seguradoras habilitadas para cotar.");
        }

        var activeInsurerIds = activeEnablements.Select(enablement => enablement.InsurerId).Distinct().ToList();

        var (targetInsurerIds, notSelectedInsurerIds) = ResolveScope(group, activeInsurerIds);

        // RN-060: substitui as Cotações anteriores do Grupo e descarta a escolha (re-solicitação).
        var existing = await quotationRepository.ListByGroupAsync(group.Id, cancellationToken);
        foreach (var previous in existing)
        {
            quotationRepository.Remove(previous);
        }

        group.ClearSelection();
        quotationGroupRepository.Update(group);

        // RN-057: uma Cotação Requested por Seguradora-alvo; RN-056: não selecionadas viram Indisponível local.
        var quotations = new List<Quotation>();
        var toEnqueue = new List<Quotation>();

        foreach (var insurerId in targetInsurerIds)
        {
            var quotation = Quotation.Requested(group.Id, insurerId);
            quotations.Add(quotation);
            toEnqueue.Add(quotation);
        }

        foreach (var insurerId in notSelectedInsurerIds)
        {
            quotations.Add(Quotation.UnavailableLocal(
                group.Id, insurerId, "Seguradora não incluída na solicitação."));
        }

        // ADR-050: persiste o estado ANTES de enfileirar — o consumidor sobrescreve cada Cotação ao chegar.
        await quotationRepository.AddRangeAsync(quotations, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        foreach (var quotation in toEnqueue)
        {
            await requestChannel.EnqueueAsync(
                new QuotationRequestWorkItem(quotation.Id, group.Id, quotation.InsurerId, request.BrokerageId),
                cancellationToken);
        }

        return new RunQuotationsResponse(group.Id, toEnqueue.Count);
    }

    private static (List<Guid> Targets, List<Guid> NotSelected) ResolveScope(
        QuotationGroup group, List<Guid> activeInsurerIds)
    {
        // RN-056: modo *todas* cota todas as habilitadas ativas.
        if (group.ScopeMode != EQuotationScopeMode.Specific)
        {
            return (activeInsurerIds, []);
        }

        // RN-056: modo *escolhidas* cota exatamente as selecionadas; as demais habilitadas viram Indisponível local.
        var selected = group.SelectedInsurers.Select(insurer => insurer.InsurerId).Distinct().ToList();

        if (selected.Count == 0)
        {
            throw new BusinessRuleException("Nenhuma Seguradora foi selecionada para cotar.");
        }

        var targets = activeInsurerIds.Where(selected.Contains).ToList();
        var notSelected = activeInsurerIds.Where(id => !selected.Contains(id)).ToList();

        return (targets, notSelected);
    }
}
