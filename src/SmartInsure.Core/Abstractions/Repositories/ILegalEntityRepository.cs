using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface ILegalEntityRepository : IRepository<LegalEntity>
{
    /// <summary>
    /// RN-013: busca por "contém" na razão social e no nome fantasia, ou por CNPJ exato
    /// (somente dígitos). RN-016: <paramref name="headquartersOnly"/> restringe a matrizes.
    /// </summary>
    Task<IReadOnlyList<LegalEntitySearchItemDto>> SearchByNameOrCnpjAsync(
        string nameTerm,
        string? cnpj,
        bool headquartersOnly,
        CancellationToken cancellationToken);

    /// <summary>RN-014: uma Pessoa Jurídica por CNPJ — consulta antes de importar.</summary>
    Task<LegalEntitySearchItemDto?> GetByCnpjAsync(string cnpj, CancellationToken cancellationToken);
}
