using SmartInsure.Core.Enumerators;

namespace SmartInsure.Core.Abstractions.Repositories.Dtos;

/// <summary>
/// RN-077: uma linha do "livro" de Cotações da Corretora — projeção achatada (ADR-038) juntando a
/// Cotação com o Grupo (Tomador, Segurado, Modalidade, IS, vigência) e o resultado classificado
/// (RN-058). O nome/logo da Seguradora é resolvido no use case por id (batch, evita N+1).
/// </summary>
public sealed record QuotationBookItemDto(
    Guid QuotationId,
    string? Number,
    string PolicyHolderName,
    string InsuredName,
    Guid InsurerId,
    string ModalityName,
    decimal InsuredAmount,
    decimal? Premium,
    decimal? CommissionPercentage,
    EQuotationResult Result,
    DateOnly CoverageStartDate,
    DateOnly CoverageEndDate,
    DateTime CreatedAt);

/// <summary>RN-078: contagem de Cotações por situação apresentada (nome estável do resultado).</summary>
public sealed record QuotationSituationCountDto(EQuotationResult Result, long Count);

/// <summary>
/// RN-077/RN-078: uma página do livro + total + contagem por situação. A contagem respeita a busca
/// e os demais filtros, mas ignora a aba de situação ativa (para as abas mostrarem o total de cada uma).
/// </summary>
public sealed record QuotationBookPageDto(
    IReadOnlyList<QuotationBookItemDto> Items,
    long TotalCount,
    IReadOnlyList<QuotationSituationCountDto> Counts);
