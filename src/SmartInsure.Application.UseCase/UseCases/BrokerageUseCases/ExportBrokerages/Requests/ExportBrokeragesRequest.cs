namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ExportBrokerages.Requests;

/// <summary>
/// RN-018 — exportação da listagem de Corretoras (.xlsx, síncrona v1): mesmos filtros
/// combinados da listagem, sem paginação (teto de segurança aplicado no use case).
/// </summary>
public sealed record ExportBrokeragesRequest
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
