using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Application.UseCase.Services.Quotations;

/// <summary>
/// RN-105/RN-106 (ADR-103): traduz as Coberturas Adicionais canônicas escolhidas no Grupo para os
/// NOMES com que a Seguradora cotada expõe as coberturas — o gateway recusa o identificador de
/// origem e reconhece a cobertura pelo nome. Cobertura sem nome resolvível, ou com nomes divergentes
/// entre ramos (OPEN-22), sai como não contemplada: nunca se envia superset, porque uma cobertura não
/// suportada faz a Seguradora recusar a solicitação inteira, derrubando a Cotação.
/// </summary>
public sealed class QuotationAdditionalCoverageResolver(
    IImportedAdditionalCoverageRepository importedAdditionalCoverageRepository)
    : IQuotationAdditionalCoverageResolver
{
    public async Task<AdditionalCoverageResolution> ResolveAsync(
        Guid insurerId,
        Guid modalityId,
        IReadOnlyCollection<Guid> additionalCoverageIds,
        CancellationToken cancellationToken)
    {
        // RN-105: Grupo sem cobertura escolhida não consulta catálogo nem grava situação.
        if (additionalCoverageIds.Count == 0)
        {
            return new AdditionalCoverageResolution([], []);
        }

        var offerable = await importedAdditionalCoverageRepository.ListForQuotationAsync(
            insurerId, modalityId, additionalCoverageIds, cancellationToken);

        var byCoverage = offerable
            .GroupBy(row => row.AdditionalCoverageId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var items = new List<ResolvedAdditionalCoverage>();
        var names = new List<string>();

        foreach (var coverageId in additionalCoverageIds.Distinct())
        {
            var rows = byCoverage.TryGetValue(coverageId, out var found) ? found : [];
            var distinctNames = rows.Select(row => row.Name).Distinct(StringComparer.Ordinal).ToList();

            // RN-106: a Seguradora não oferece (nenhum nome) ou o nome divergiu entre ramos
            // (mais de um nome distinto — OPEN-22). Nos dois casos, não contemplada.
            if (distinctNames.Count != 1)
            {
                items.Add(new ResolvedAdditionalCoverage(
                    coverageId, EQuotationAdditionalCoverageStatus.NotOffered, null, null));
                continue;
            }

            var name = distinctNames[0];

            // Ramos que compartilham o nome deixam a Importada de origem indeterminada — o nome, que
            // é o que vai à Seguradora, continua inequívoco.
            var importedId = rows.Count == 1 ? rows[0].ImportedAdditionalCoverageId : (Guid?)null;

            items.Add(new ResolvedAdditionalCoverage(
                coverageId, EQuotationAdditionalCoverageStatus.Sent, name, importedId));
            names.Add(name);
        }

        return new AdditionalCoverageResolution(
            names.Distinct(StringComparer.Ordinal).ToList(),
            items);
    }
}
