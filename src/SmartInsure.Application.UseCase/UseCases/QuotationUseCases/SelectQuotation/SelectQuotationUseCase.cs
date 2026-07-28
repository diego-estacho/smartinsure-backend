using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SelectQuotation.Interfaces;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SelectQuotation.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SelectQuotation.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SelectQuotation;

/// <summary>
/// RN-059: marca a Cotação escolhida do Grupo. São seguíveis apenas as Automáticas e as em Análise de
/// Subscrição (a exigência de CCG não bloqueia — ADR-064); as demais são recusadas. Substitui a escolha anterior.
/// </summary>
public sealed class SelectQuotationUseCase(
    IQuotationRepository quotationRepository,
    IQuotationGroupRepository groupRepository,
    IUnitOfWork unitOfWork) : ISelectQuotationUseCase
{
    public async Task<SelectQuotationResponse> ExecuteAsync(
        SelectQuotationRequest request, CancellationToken cancellationToken)
    {
        var quotation = await quotationRepository.GetByIdAsync(request.QuotationId, cancellationToken)
            ?? throw new NotFoundException("Cotação não encontrada.");

        if (quotation.QuotationGroupId != request.QuotationGroupId)
        {
            throw new BusinessRuleException("A Cotação não pertence ao Grupo de Cotação informado.");
        }

        if (!quotation.IsFollowable)
        {
            throw new BusinessRuleException("Esta Cotação não pode ser escolhida para seguir nesta fase.");
        }

        var group = await groupRepository.GetByIdWithInsurersAsync(request.QuotationGroupId, cancellationToken)
            ?? throw new NotFoundException("Grupo de Cotação não encontrado.");

        group.SelectQuotation(quotation.Id);
        await unitOfWork.CommitAsync(cancellationToken);

        return new SelectQuotationResponse(group.Id, quotation.Id);
    }
}
