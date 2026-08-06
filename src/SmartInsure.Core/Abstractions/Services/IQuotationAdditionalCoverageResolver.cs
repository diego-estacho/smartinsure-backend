using SmartInsure.Core.Enumerators;

namespace SmartInsure.Core.Abstractions.Services;

/// <summary>
/// RN-105/RN-106 (ADR-103): traduz as Coberturas Adicionais canônicas escolhidas no Grupo para os
/// NOMES com que UMA Seguradora expõe as coberturas na Modalidade cotada, e informa quais ela não
/// oferece. O gateway recusa o identificador de origem e reconhece a cobertura pelo nome.
/// É regra de negócio (implementada na Application) — não vive na camada anticorrupção do motor,
/// que só traduz modelo de fornecedor e não depende de repositório de catálogo (ADR-045, ADR-028).
/// </summary>
public interface IQuotationAdditionalCoverageResolver
{
    Task<AdditionalCoverageResolution> ResolveAsync(
        Guid insurerId,
        Guid modalityId,
        IReadOnlyCollection<Guid> additionalCoverageIds,
        CancellationToken cancellationToken);
}

/// <summary>
/// Nomes a enviar à Seguradora (RN-105) e a situação de cada Cobertura Adicional escolhida (RN-106).
/// </summary>
public sealed record AdditionalCoverageResolution(
    IReadOnlyList<string> NamesToSend,
    IReadOnlyList<ResolvedAdditionalCoverage> Items);

/// <summary>
/// Situação de UMA Cobertura Adicional escolhida para UMA Seguradora (RN-106).
/// <paramref name="SentName"/> é o nome enviado (nulo quando não contemplada) e
/// <paramref name="ImportedAdditionalCoverageId"/> a Importada de origem, quando identificável.
/// </summary>
public sealed record ResolvedAdditionalCoverage(
    Guid AdditionalCoverageId,
    EQuotationAdditionalCoverageStatus Status,
    string? SentName,
    Guid? ImportedAdditionalCoverageId);
