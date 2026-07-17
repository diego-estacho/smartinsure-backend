using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

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

    /// <summary>RN-017: entidade rastreada para atribuição de papel via change tracker.</summary>
    Task<Person?> GetTrackedByDocumentNumberAsync(
        string documentNumber, CancellationToken cancellationToken);

    /// <summary>RN-018: lista Pessoas jurídicas com Papel da Pessoa de corretor.</summary>
    Task<(IReadOnlyList<BrokerageListItemDto> Items, long TotalCount)> ListBrokeragesAsync(
        int page,
        int pageSize,
        EPersonRoleStatus? status,
        CancellationToken cancellationToken);

    /// <summary>RN-020: detalhe da Corretora a partir da Pessoa jurídica com papel Corretor.</summary>
    Task<BrokerageDetailsDto?> GetBrokerageByIdAsync(
        Guid personId,
        CancellationToken cancellationToken);

    /// <summary>RN-021: Pessoa rastreada com o papel Corretor para alterar situação.</summary>
    Task<Person?> GetTrackedBrokerageByIdAsync(
        Guid personId,
        CancellationToken cancellationToken);
}
