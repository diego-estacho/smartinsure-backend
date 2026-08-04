using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotationBook.Interfaces;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotationBook.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotationBook.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotationBook;

/// <summary>
/// RN-077/RN-078 — o "livro" de Cotações da Corretora do Escopo ativo (RN-064). Lê o estado persistido já
/// classificado (RN-058), sem cotar: a inclusão (só Obtained com resultado do provedor), o escopo, a
/// busca, os filtros, a contagem por situação e as opções vivem na projeção do repositório. Aqui só
/// resolvemos o Escopo (fail-closed), saneamos a paginação, traduzimos a situação por **nome estável**
/// (ADR-031) e montamos a resposta.
/// </summary>
public sealed class ListQuotationBookUseCase(IQuotationRepository quotationRepository) : IListQuotationBookUseCase
{
    public async Task<QuotationBookResponse> ExecuteAsync(
        ListQuotationBookRequest request, CancellationToken cancellationToken)
    {
        // RN-064/SECURITY.md: sem Corretora ativa não há livro a consultar (fail-closed).
        var brokerageId = request.ActiveBrokerageId
            ?? throw new ForbiddenException("Selecione a corretora ativa para consultar as cotações.");

        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var filter = new QuotationBookFilter(
            brokerageId,
            page,
            pageSize,
            request.Search,
            ParseSituation(request.Situation),
            request.InsurerId,
            request.ModalityId,
            request.PremiumMin,
            request.PremiumMax,
            request.InsuredAmountMin,
            request.InsuredAmountMax,
            request.CreatedFrom,
            request.CreatedTo,
            request.CoverageStartFrom,
            request.CoverageStartTo);

        var pageDto = await quotationRepository.ListBookAsync(filter, cancellationToken);

        var items = pageDto.Items
            .Select(item => new QuotationBookItemResponse(
                item.QuotationId,
                item.Number,
                item.PolicyHolderName,
                item.InsuredName,
                item.InsurerId,
                item.InsurerName,
                item.InsurerLogoUrl,
                item.ModalityId,
                item.ModalityName,
                item.InsuredAmount,
                item.Premium,
                item.CommissionPercentage,
                item.Result.ToString(),
                item.RequiresCcg,
                item.CoverageStartDate,
                item.CoverageEndDate,
                item.CreatedAt))
            .ToList();

        var counts = pageDto.Counts
            .Select(count => new QuotationSituationCountResponse(count.Result.ToString(), count.Count))
            .ToList();

        var insurers = pageDto.Insurers
            .Select(option => new QuotationBookOptionResponse(option.Id, option.Name))
            .ToList();

        var modalities = pageDto.Modalities
            .Select(option => new QuotationBookOptionResponse(option.Id, option.Name))
            .ToList();

        return new QuotationBookResponse(items, page, pageSize, pageDto.TotalCount, counts, insurers, modalities);
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
