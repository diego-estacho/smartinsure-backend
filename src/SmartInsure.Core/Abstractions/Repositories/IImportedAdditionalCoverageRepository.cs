using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IImportedAdditionalCoverageRepository : IRepository<ImportedAdditionalCoverage>
{
    /// <summary>RN-041: reencontra a importada por (Modalidade Importada, nome) — rastreada, para upsert.</summary>
    Task<ImportedAdditionalCoverage?> GetByImportedModalityAndNameAsync(
        Guid importedModalityId, string name, CancellationToken cancellationToken);

    /// <summary>RN-044: importadas Ativas de uma Modalidade Importada (rastreadas), para desativar o que sumiu.</summary>
    Task<IReadOnlyList<ImportedAdditionalCoverage>> ListActiveByImportedModalityAsync(
        Guid importedModalityId, CancellationToken cancellationToken);

    /// <summary>RN-043: pendências de mapeamento — Ativas, não Ignoradas, sem vínculo (a Fila da curadoria).</summary>
    Task<IReadOnlyList<PendingImportedCoverageDto>> ListPendingAsync(CancellationToken cancellationToken);

    /// <summary>RN-043/RN-046: importadas Ativas vinculadas a uma Cobertura Adicional canônica (a matriz da curadoria).</summary>
    Task<IReadOnlyList<LinkedImportedCoverageDto>> ListLinkedAsync(CancellationToken cancellationToken);

    /// <summary>
    /// RN-105: importadas Ativas vinculadas às canônicas escolhidas, nas Modalidades Importadas Ativas
    /// e não Ignoradas de UMA Seguradora vinculadas à Modalidade cotada. O nome devolvido é o que vai
    /// à Seguradora (ADR-103).
    /// </summary>
    Task<IReadOnlyList<OfferableImportedCoverageDto>> ListForQuotationAsync(
        Guid insurerId,
        Guid modalityId,
        IReadOnlyCollection<Guid> additionalCoverageIds,
        CancellationToken cancellationToken);

    /// <summary>
    /// RN-104: canônicas Ativas ofertáveis para uma Modalidade, considerando só Seguradoras com
    /// Habilitação Ativa da Corretora informada. Usa o MESMO critério de derivação de
    /// <see cref="ListForQuotationAsync"/>, para que oferta e envio nunca divirjam.
    /// </summary>
    Task<IReadOnlyList<AvailableAdditionalCoverageDto>> ListAvailableForModalityAsync(
        Guid brokerageId,
        Guid modalityId,
        CancellationToken cancellationToken);
}
