namespace SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ExecuteCreditInquiry.Responses;

/// <summary>RN-029/RN-031: resultado da execução de consulta de limites de crédito.</summary>
public sealed record ExecuteCreditInquiryResponse(
    Guid CreditInquiryId,
    DateTime QueriedAt,
    string PolicyHolderCnpj,
    string? PolicyHolderName,
    CreditInquirySummary Summary,
    IReadOnlyList<CreditInquiryResultResponse> Results);

/// <summary>Resumo consolidado da consulta (RN-030: apenas seguradoras com resultado).</summary>
public sealed record CreditInquirySummary(
    int InsurersQueried,
    int InsurersAvailable,
    decimal ConsolidatedLimit);

/// <summary>Resultado individual por Seguradora (status, limites por grupo de modalidade).</summary>
public sealed record CreditInquiryResultResponse(
    Guid InsurerId,
    string InsurerName,
    string Status,
    string? FailureReason,
    string? PolicyHolderName,
    IReadOnlyList<CreditInquiryLimitGroupResponse> Limits);

/// <summary>Limite de crédito agrupado por grupo de modalidade (RN-029).</summary>
public sealed record CreditInquiryLimitGroupResponse(
    string GroupName,
    string GroupType,
    decimal AvailableLimit,
    decimal UsedLimit,
    decimal Rate);
