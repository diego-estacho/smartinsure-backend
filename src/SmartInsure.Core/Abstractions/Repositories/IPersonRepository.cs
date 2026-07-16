using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IPersonRepository : IRepository<Person>
{
    /// <summary>
    /// RN-013: busca por "contém" na razão social e no nome fantasia, ou por CNPJ exato
    /// (somente dígitos). RN-016: <paramref name="headquartersOnly"/> restringe a matrizes.
    /// </summary>
    Task<IReadOnlyList<PersonSearchItemDto>> SearchByNameOrCnpjAsync(
        string nameTerm,
        string? cnpj,
        bool headquartersOnly,
        CancellationToken cancellationToken);

    /// <summary>RN-014: uma Pessoa por CNPJ — consulta antes de importar.</summary>
    Task<PersonSearchItemDto?> GetByCnpjAsync(string cnpj, CancellationToken cancellationToken);
}
