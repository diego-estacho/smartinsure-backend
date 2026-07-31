namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotationBook.Requests;

/// <summary>
/// RN-077: consulta do livro de Cotações da Corretora do Escopo ativo. A Corretora vem do acesso
/// (RN-064), nunca do corpo/query (SECURITY.md). Primeiro corte: paginação + busca + situação.
/// </summary>
public sealed record ListQuotationBookRequest
{
    /// <summary>Corretora ativa do acesso corrente (RN-064); ausente → consulta recusada.</summary>
    public Guid? ActiveBrokerageId { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    /// <summary>Busca livre: número, Tomador, Segurado, Modalidade (Seguradora entra na fatia seguinte).</summary>
    public string? Search { get; set; }

    /// <summary>Situação apresentada pelo nome estável do resultado (ReadyForEmission/Analysis/Unavailable/Unrecognized).</summary>
    public string? Situation { get; set; }
}
