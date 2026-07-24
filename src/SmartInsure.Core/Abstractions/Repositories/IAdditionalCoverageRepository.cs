using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IAdditionalCoverageRepository : IRepository<AdditionalCoverage>
{
    /// <summary>RN-040: reencontra a Cobertura Adicional canônica pelo nome (rastreada), para checar unicidade.</summary>
    Task<AdditionalCoverage?> GetByNameAsync(string name, CancellationToken cancellationToken);

    /// <summary>RN-040: catálogo canônico para a curadoria (o Mapa da Cobertura Adicional).</summary>
    Task<IReadOnlyList<AdditionalCoverageListItemDto>> ListAllAsync(CancellationToken cancellationToken);
}
