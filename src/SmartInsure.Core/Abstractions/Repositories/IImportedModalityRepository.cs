using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IImportedModalityRepository : IRepository<ImportedModality>
{
    /// <summary>RN-030: reencontra a Modalidade Importada pelo identificador de origem, por Seguradora (rastreada, para upsert).</summary>
    Task<ImportedModality?> GetByInsurerAndSourceAsync(
        Guid insurerId, string sourceId, CancellationToken cancellationToken);

    /// <summary>RN-035: Modalidades Importadas Ativas da Seguradora (rastreadas), para desativar o que sumiu.</summary>
    Task<IReadOnlyList<ImportedModality>> ListActiveByInsurerAsync(
        Guid insurerId, CancellationToken cancellationToken);

    /// <summary>
    /// RN-032: dado o identificador do motor e o ramo, retorna a Modalidade (Smart) que outra
    /// Modalidade Importada do mesmo identificador e ramo já tem confirmada — nunca cruza ramos.
    /// </summary>
    Task<Guid?> FindConfirmedModalityIdByEngineAsync(
        string engineModalityId, ESuretyBranch branch, CancellationToken cancellationToken);
}
