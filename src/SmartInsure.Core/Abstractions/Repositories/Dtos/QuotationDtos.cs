using SmartInsure.Core.Enumerators;

namespace SmartInsure.Core.Abstractions.Repositories.Dtos;

/// <summary>
/// RN-077: filtros do "livro" de Cotações, todos por E lógico. A Corretora (Escopo ativo) é obrigatória.
/// Datas por período (De/Até); faixas por mín/máx. Nulo = sem aquele filtro.
/// </summary>
public sealed record QuotationBookFilter(
    Guid BrokerageId,
    int Page,
    int PageSize,
    string? Search,
    EQuotationResult? Situation,
    Guid? InsurerId,
    Guid? ModalityId,
    decimal? PremiumMin,
    decimal? PremiumMax,
    decimal? InsuredAmountMin,
    decimal? InsuredAmountMax,
    DateOnly? CreatedFrom,
    DateOnly? CreatedTo,
    DateOnly? CoverageStartFrom,
    DateOnly? CoverageStartTo);

/// <summary>
/// RN-077: uma linha do "livro" — projeção achatada (ADR-038) da Cotação com o Grupo (Tomador, Segurado,
/// Modalidade, IS, vigência), a Seguradora (nome/logo) e o resultado classificado (RN-058).
/// </summary>
public sealed record QuotationBookItemDto(
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
    EQuotationResult Result,
    bool RequiresCcg,
    DateOnly CoverageStartDate,
    DateOnly CoverageEndDate,
    DateTime CreatedAt);

/// <summary>RN-078: contagem de Cotações por situação apresentada (nome estável do resultado).</summary>
public sealed record QuotationSituationCountDto(EQuotationResult Result, long Count);

/// <summary>RN-077: opção de filtro (Seguradora ou Modalidade) presente no livro da Corretora.</summary>
public sealed record QuotationBookOptionDto(Guid Id, string Name);

/// <summary>
/// RN-077/RN-078: página do livro + total + contagem por situação + opções de filtro. A contagem respeita
/// a busca e os filtros avançados, mas ignora a aba de situação; as opções são os distintos presentes no
/// livro (independentes dos demais filtros — Q10).
/// </summary>
public sealed record QuotationBookPageDto(
    IReadOnlyList<QuotationBookItemDto> Items,
    long TotalCount,
    IReadOnlyList<QuotationSituationCountDto> Counts,
    IReadOnlyList<QuotationBookOptionDto> Insurers,
    IReadOnlyList<QuotationBookOptionDto> Modalities);
