using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IImportedModalityRepository : IRepository<ImportedModality>
{
    /// <summary>RN-030: reencontra a Modalidade Importada pelo identificador de origem, por Seguradora (rastreada, para upsert).</summary>
    Task<ImportedModality?> GetByInsurerAndSourceAsync(
        Guid insurerId, string sourceId, CancellationToken cancellationToken);

    /// <summary>RN-035: Modalidades Importadas Ativas da Seguradora (rastreadas), para desativar o que sumiu.</summary>
    Task<IReadOnlyList<ImportedModality>> ListActiveByInsurerAsync(
        Guid insurerId, CancellationToken cancellationToken);

    /// <summary>RN-033: vínculos ativos (Importada Ativa, não Ignorada, com Modalidade) — a matriz do Mapa.</summary>
    Task<IReadOnlyList<ModalityInsurerLinkDto>> ListActiveLinksAsync(CancellationToken cancellationToken);

    /// <summary>RN-034: pendências da Fila — Ativas, não Ignoradas, sem vínculo (ModalityId nulo).</summary>
    Task<IReadOnlyList<PendingImportedModalityDto>> ListPendingAsync(CancellationToken cancellationToken);
}
