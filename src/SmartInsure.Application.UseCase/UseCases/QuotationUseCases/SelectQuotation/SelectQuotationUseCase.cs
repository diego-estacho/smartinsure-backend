using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SelectQuotation.Interfaces;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SelectQuotation.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SelectQuotation.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SelectQuotation;

/// <summary>
/// RN-059 — Seleção da Cotação para seguir: marca a Cotação escolhida do Grupo. Só são seguíveis as
/// Cotações Automáticas e as em Análise de subscrição (Quotation.IsFollowable); as demais são
/// recusadas. No máximo uma escolhida por Grupo — escolher outra substitui a anterior. A exigência de
/// CCG não bloqueia a seleção (ADR-064); é enforçada só na emissão (fora desta fase).
/// </summary>
public sealed class SelectQuotationUseCase(
    IQuotationGroupRepository quotationGroupRepository,
    IQuotationRepository quotationRepository,
    IUnitOfWork unitOfWork) : ISelectQuotationUseCase
{
    public async Task<SelectQuotationResponse> ExecuteAsync(
        SelectQuotationRequest request,
        CancellationToken cancellationToken)
    {
        var group = await quotationGroupRepository.GetByIdAsync(request.QuotationGroupId, cancellationToken)
            ?? throw new NotFoundException("Grupo de Cotação não encontrado.");

        var quotation = await quotationRepository.GetByIdAsync(request.QuotationId, cancellationToken)
            ?? throw new NotFoundException("Cotação não encontrada.");

        // RN-059: a Cotação escolhida tem de pertencer a este Grupo.
        if (quotation.QuotationGroupId != group.Id)
        {
            throw new BusinessRuleException("A Cotação não pertence a este Grupo de Cotação.");
        }

        // RN-059: só Automática ou Análise de subscrição são seguíveis; as demais não são selecionáveis.
        if (!quotation.IsFollowable)
        {
            throw new BusinessRuleException("Esta Cotação não pode ser selecionada para seguir.");
        }

        // RN-059: no máximo uma escolhida por Grupo; escolher outra substitui a anterior.
        group.SelectQuotation(quotation.Id);
        quotationGroupRepository.Update(group);
        await unitOfWork.CommitAsync(cancellationToken);

        return new SelectQuotationResponse(group.Id, quotation.Id);
    }
}
