using SmartInsure.Application.UseCase.ModelsBase;

namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ListBrokerages.Requests;

/// <summary>
/// RN-018 — lista Corretoras com busca livre e filtros combinados (situação apresentada,
/// Seguradora habilitada, Motor de Cálculo, setor e período de cadastro), tudo server-side.
/// </summary>
public sealed record ListBrokeragesRequest : PagedRequest
{
    /// <summary>Busca por CNPJ (dígitos), razão social ou nome fantasia.</summary>
    public string? Search { get; init; }

    /// <summary>Situação apresentada: Active, Incomplete ou Inactive (RN-053).</summary>
    public string? Situation { get; init; }

    public Guid? InsurerId { get; init; }

    public string? CalculationEngine { get; init; }

    /// <summary>Setor: Public ou Private (pela Natureza Jurídica).</summary>
    public string? Sector { get; init; }

    public DateTime? RegisteredFrom { get; init; }

    public DateTime? RegisteredTo { get; init; }
}
