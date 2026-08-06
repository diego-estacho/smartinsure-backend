namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotationBook.Requests;

/// <summary>
/// RN-077: consulta do livro de Cotações da Corretora do Escopo ativo. A Corretora vem do acesso
/// (RN-064), nunca do corpo/query (SECURITY.md). Paginação + busca + situação + filtros avançados.
/// </summary>
public sealed record ListQuotationBookRequest
{
    /// <summary>Corretora ativa do acesso corrente (RN-064); ausente → consulta recusada.</summary>
    public Guid? ActiveBrokerageId { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    /// <summary>Busca livre: número, Tomador, Segurado, Seguradora, Modalidade.</summary>
    public string? Search { get; set; }

    /// <summary>Situação apresentada pelo nome estável do resultado (ReadyForEmission/Analysis/Unavailable/Unrecognized).</summary>
    public string? Situation { get; set; }

    public Guid? InsurerId { get; set; }

    public Guid? ModalityId { get; set; }

    public decimal? PremiumMin { get; set; }

    public decimal? PremiumMax { get; set; }

    public decimal? InsuredAmountMin { get; set; }

    public decimal? InsuredAmountMax { get; set; }

    public DateOnly? CreatedFrom { get; set; }

    public DateOnly? CreatedTo { get; set; }

    public DateOnly? CoverageStartFrom { get; set; }

    public DateOnly? CoverageStartTo { get; set; }
}
