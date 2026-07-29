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
}
