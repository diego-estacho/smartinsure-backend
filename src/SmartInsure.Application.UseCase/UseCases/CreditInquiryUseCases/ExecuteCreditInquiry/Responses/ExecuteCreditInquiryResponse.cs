namespace SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ExecuteCreditInquiry.Responses;

/// <summary>RN-029/RN-031: resultado da execução de consulta de limites de crédito.</summary>
public sealed record ExecuteCreditInquiryResponse(
    Guid CreditInquiryId,
    DateTime QueriedAt,
    string PolicyHolderCnpj,
    CreditInquirySummary Summary,
    IReadOnlyList<CreditInquiryResultResponse> Results);

/// <summary>Resumo consolidado da consulta (RN-030: apenas seguradoras com resultado).</summary>
public sealed record CreditInquirySummary(
    int InsurersQueried,
    int InsurersAvailable,
    decimal ConsolidatedLimit);

/// <summary>Resultado individual por Seguradora (status, limites, taxas, validade).</summary>
public sealed record CreditInquiryResultResponse(
    Guid InsurerId,
    string InsurerName,
    string Status,
    string? FailureReason,
    decimal? TraditionalLimit,
    decimal? TraditionalRate,
    decimal? JudicialLimit,
    decimal? JudicialRate,
    decimal? JudicialFiscalRate,
    decimal? FinancialLimit,
    decimal? FinancialRate,
    DateTime? LimitValidUntil);
