using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

/// <summary>Repositório de Consultas de Crédito (RN-029..031).</summary>
public interface ICreditInquiryRepository
{
    /// <summary>RN-031: persiste consulta de crédito com seus resultados.</summary>
    Task AddAsync(CreditInquiry inquiry, CancellationToken cancellationToken);

    /// <summary>RN-031: recupera consulta por id.</summary>
    Task<CreditInquiry?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>RN-031: lista consultas paginadas com filtros opcionais por brokerage e CNPJ.</summary>
    Task<(IReadOnlyList<CreditInquiryListItem> Items, long TotalCount)> ListPagedAsync(
        Guid? brokerageId,
        string? policyHolderCnpj,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}

/// <summary>Modelo de leitura para listagem de Consultas de Crédito (projeção).</summary>
public sealed record CreditInquiryListItem(
    Guid Id,
    Guid BrokerageId,
    string PolicyHolderCnpj,
    DateTime QueriedAt,
    int ResultsCount,
    int AvailableResults);
