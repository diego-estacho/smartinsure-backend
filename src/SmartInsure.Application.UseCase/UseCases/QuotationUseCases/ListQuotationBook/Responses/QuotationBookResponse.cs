namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotationBook.Responses;

/// <summary>
/// RN-077: uma linha do livro de Cotações. O resultado sai pelo **nome estável** (ADR-031); o rótulo da
/// situação apresentada (RN-078) é montado na apresentação. Número vazio quando a Seguradora não
/// informou; prêmio/comissão só quando aplicáveis (RN-058).
/// </summary>
public sealed record QuotationBookItemResponse(
    Guid QuotationId,
    string? Number,
    string PolicyHolderName,
    string InsuredName,
    Guid InsurerId,
    string InsurerName,
    string? InsurerLogoUrl,
    Guid ModalityId,
    string ModalityName,
    decimal InsuredAmount,
    decimal? Premium,
    decimal? CommissionPercentage,
    string Result,
    bool RequiresCcg,
    DateOnly CoverageStartDate,
    DateOnly CoverageEndDate,
    DateTime CreatedAt);

/// <summary>RN-078: contagem por situação apresentada (nome estável do resultado), para as abas.</summary>
public sealed record QuotationSituationCountResponse(string Result, long Count);

/// <summary>RN-077: opção de filtro (Seguradora ou Modalidade) presente no livro.</summary>
public sealed record QuotationBookOptionResponse(Guid Id, string Name);

/// <summary>
/// RN-077: página do livro + total + contagem por situação (respeita busca/filtros, ignora a aba) +
/// opções de filtro (distintos presentes no livro).
/// </summary>
public sealed record QuotationBookResponse(
    IReadOnlyList<QuotationBookItemResponse> Items,
    int Page,
    int PageSize,
    long TotalCount,
    IReadOnlyList<QuotationSituationCountResponse> Counts,
    IReadOnlyList<QuotationBookOptionResponse> Insurers,
    IReadOnlyList<QuotationBookOptionResponse> Modalities);
