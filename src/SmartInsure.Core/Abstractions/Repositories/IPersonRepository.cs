using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

public interface IPersonRepository : IRepository<Person>
{
    /// <summary>
    /// RN-013: busca por "contém" no nome e no nome social, ou por documento exato
    /// (CPF/CNPJ, somente dígitos). RN-016: <paramref name="headquartersOnly"/>
    /// restringe a matrizes (pessoas jurídicas de ordem /0001).
    /// </summary>
    Task<IReadOnlyList<PersonSearchItemDto>> SearchByNameOrDocumentAsync(
        string nameTerm,
        string? documentNumber,
        bool headquartersOnly,
        CancellationToken cancellationToken);

    /// <summary>RN-014: uma Pessoa por documento — consulta antes de importar.</summary>
    Task<PersonSearchItemDto?> GetByDocumentNumberAsync(
        string documentNumber, CancellationToken cancellationToken);
}
