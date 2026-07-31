using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotationBook.Interfaces;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotationBook.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotationBook.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotationBook;

/// <summary>
/// RN-077/RN-078 — o "livro" de Cotações da Corretora do Escopo ativo (RN-064). Lê o estado persistido
/// já classificado (RN-058), sem cotar: só as Cotações obtidas com resultado do provedor (a inclusão e
/// o escopo vivem na projeção do repositório). A situação apresentada sai pelo **nome estável** do
/// resultado (ADR-031); o rótulo pt-BR é montado na apresentação. Nome/logo da Seguradora resolvidos
/// por id em lote (evita N+1), como no leque (RN-057).
/// </summary>
public sealed class ListQuotationBookUseCase(
    IQuotationRepository quotationRepository,
    IInsurerRepository insurerRepository) : IListQuotationBookUseCase
{
    public async Task<QuotationBookResponse> ExecuteAsync(
        ListQuotationBookRequest request, CancellationToken cancellationToken)
    {
        // RN-064/SECURITY.md: sem Corretora ativa não há livro a consultar (fail-closed).
        var brokerageId = request.ActiveBrokerageId
            ?? throw new ForbiddenException("Selecione a corretora ativa para consultar as cotações.");

        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var situation = ParseSituation(request.Situation);

        var pageDto = await quotationRepository.ListBookAsync(
            brokerageId, page, pageSize, request.Search, situation, cancellationToken);

        var insurerIds = pageDto.Items.Select(item => item.InsurerId).Distinct().ToList();
        var insurerNames = await insurerRepository.GetCorporateNamesByIdsAsync(insurerIds, cancellationToken);
        var insurerLogos = await insurerRepository.GetLogoUrlsByIdsAsync(insurerIds, cancellationToken);

        var items = pageDto.Items
            .Select(item => new QuotationBookItemResponse(
                item.QuotationId,
                item.Number,
                item.PolicyHolderName,
                item.InsuredName,
                item.InsurerId,
                insurerNames.TryGetValue(item.InsurerId, out var name) ? name : "Seguradora",
                insurerLogos.TryGetValue(item.InsurerId, out var logo) ? logo : null,
                item.ModalityName,
                item.InsuredAmount,
                item.Premium,
                item.CommissionPercentage,
                item.Result.ToString(),
                item.CoverageStartDate,
                item.CoverageEndDate,
                item.CreatedAt))
            .ToList();

        var counts = pageDto.Counts
            .Select(count => new QuotationSituationCountResponse(count.Result.ToString(), count.Count))
            .ToList();

        return new QuotationBookResponse(items, page, pageSize, pageDto.TotalCount, counts);
    }

    /// <summary>
    /// RN-078: filtro de situação pelo **nome estável** do resultado (ADR-031); valor fora do enum é
    /// recusado (nunca silencia num "sem filtro").
    /// </summary>
    private static EQuotationResult? ParseSituation(string? situation)
    {
        if (string.IsNullOrWhiteSpace(situation))
        {
            return null;
        }

        if (!Enum.TryParse<EQuotationResult>(situation.Trim(), ignoreCase: true, out var parsed))
        {
            throw new BusinessRuleException($"Situação de cotação inválida: {situation}.");
        }

        return parsed;
    }
}
