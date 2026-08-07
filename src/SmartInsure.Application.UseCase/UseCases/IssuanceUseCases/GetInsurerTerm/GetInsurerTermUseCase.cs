using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.GetInsurerTerm.Interfaces;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.GetInsurerTerm.Requests;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.GetInsurerTerm.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.GetInsurerTerm;

/// <summary>
/// RN-506 — Termo e declaração vigente da Seguradora da Cotação escolhida. O texto vem do servidor
/// porque é o mesmo que o aceite registra: cliente com texto próprio abriria divergência entre o que
/// foi exibido e o que ficou assinado. Seguradora sem Termo vigente não é emitível, e isso é dito aqui,
/// antes de o corretor preencher a tela.
/// </summary>
public sealed class GetInsurerTermUseCase(
    IQuotationGroupRepository quotationGroupRepository,
    IQuotationRepository quotationRepository,
    IInsurerTermRepository insurerTermRepository) : IGetInsurerTermUseCase
{
    public async Task<GetInsurerTermResponse> ExecuteAsync(
        GetInsurerTermRequest request, CancellationToken cancellationToken)
    {
        var group = await quotationGroupRepository.GetByIdAsync(request.QuotationGroupId, cancellationToken)
            ?? throw new NotFoundException("Grupo de cotação não encontrado.");

        if (group.SelectedQuotationId is null)
        {
            throw new BusinessRuleException("Nenhuma cotação foi escolhida nesta oferta.");
        }

        var quotation = await quotationRepository.GetByIdAsync(group.SelectedQuotationId.Value, cancellationToken)
            ?? throw new NotFoundException("Cotação escolhida não encontrada.");

        var term = await insurerTermRepository.GetActiveByInsurerAsync(quotation.InsurerId, cancellationToken)
            ?? throw new BusinessRuleException(
                "A Seguradora não tem Termo e declaração vigente cadastrado — emissão indisponível.");

        return new GetInsurerTermResponse(quotation.InsurerId, term.Content);
    }
}
