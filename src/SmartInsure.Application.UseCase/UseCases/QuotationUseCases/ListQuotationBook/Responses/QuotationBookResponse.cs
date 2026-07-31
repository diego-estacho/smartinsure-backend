namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotationBook.Responses;

/// <summary>
/// RN-077: uma linha do livro de Cotações. O resultado sai pelo **nome estável** (ADR-031); o rótulo
/// da situação apresentada (RN-078) é montado na apresentação (front). Número vazio quando a
/// Seguradora não informou. Prêmio/comissão só vêm quando aplicáveis (RN-058).
/// </summary>
public sealed record QuotationBookItemResponse(
    Guid QuotationId,
    string? Number,
    string PolicyHolderName,
    string InsuredName,
    Guid InsurerId,
    string InsurerName,
    string? InsurerLogoUrl,
    string ModalityName,
    decimal InsuredAmount,
    decimal? Premium,
    decimal? CommissionPercentage,
    string Result,
    DateOnly CoverageStartDate,
    DateOnly CoverageEndDate,
    DateTime CreatedAt);

/// <summary>RN-078: contagem por situação apresentada (nome estável do resultado), para as abas.</summary>
public sealed record QuotationSituationCountResponse(string Result, long Count);

/// <summary>RN-077: página do livro + total + contagem por situação (respeita a busca, ignora a aba ativa).</summary>
public sealed record QuotationBookResponse(
    IReadOnlyList<QuotationBookItemResponse> Items,
    int Page,
    int PageSize,
    long TotalCount,
    IReadOnlyList<QuotationSituationCountResponse> Counts);
