using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ListBrokerages.Interfaces;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ListBrokerages.Requests;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ListBrokerages.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ListBrokerages;

/// <summary>
/// RN-018 — lista Pessoas jurídicas com papel Corretor, com busca e filtros combinados server-side,
/// e a contagem por situação apresentada (RN-053) para as abas. Filtro/ordenação/paginação no banco.
/// </summary>
public sealed class ListBrokeragesUseCase(IPersonRepository personRepository) : IListBrokeragesUseCase
{
    public async Task<ListBrokeragesResponse> ExecuteAsync(
        ListBrokeragesRequest request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = new BrokerageListQuery(
            page,
            pageSize,
            request.Search,
            ParseSituation(request.Situation),
            request.InsurerId,
            ParseCalculationEngine(request.CalculationEngine),
            ParseSector(request.Sector),
            request.RegisteredFrom?.Date,
            request.RegisteredTo?.Date.AddDays(1).AddTicks(-1));

        var result = await personRepository.ListBrokeragesAsync(query, cancellationToken);

        var items = result.Items
            .Select(item => new BrokerageListItemResponse(
                item.Id,
                item.DocumentNumber,
                item.Name,
                item.SocialName,
                item.IsPrivateSector,
                item.Status,
                item.Situation,
                item.RegisteredAt,
                item.EnabledInsurerCount,
                item.EnabledInsurerNames,
                item.CalculationEngines))
            .ToList();

        return new ListBrokeragesResponse(
            items,
            page,
            pageSize,
            result.TotalCount,
            new BrokerageSituationCountsResponse(
                result.Counts.All,
                result.Counts.Active,
                result.Counts.Incomplete,
                result.Counts.Inactive));
    }

    private static EBrokerageSituation? ParseSituation(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Enum.TryParse<EBrokerageSituation>(value, ignoreCase: true, out var parsed)
            ? parsed
            : throw new BusinessRuleException("A situação deve ser Active, Incomplete ou Inactive.");
    }

    private static ECalculationEngine? ParseCalculationEngine(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Enum.TryParse<ECalculationEngine>(value, ignoreCase: true, out var parsed)
            ? parsed
            : throw new BusinessRuleException("Motor de cálculo inválido.");
    }

    private static bool? ParseSector(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "private" or "privado" => true,
            "public" or "publico" or "público" => false,
            _ => throw new BusinessRuleException("O setor deve ser Public ou Private."),
        };
    }
}
