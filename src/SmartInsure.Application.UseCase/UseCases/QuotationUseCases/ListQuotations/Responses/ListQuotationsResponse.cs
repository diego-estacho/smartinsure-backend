namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotations.Responses;

/// <summary>
/// RN-057/RN-058: o leque de Cotações do Grupo para acompanhamento (polling). O estado por Seguradora
/// preenche progressivamente; SelectedQuotationId indica a escolhida (RN-059).
/// </summary>
public sealed record ListQuotationsResponse(
    Guid QuotationGroupId,
    Guid? SelectedQuotationId,
    IReadOnlyList<QuotationListItemResponse> Quotations);

/// <summary>
/// RN-058: uma Cotação no leque — classificação estável + esteira + motivos + prêmio/limite + CCG. Sem
/// prêmio quando não Pronta para emissão; a esteira específica quando em Análise; os motivos quando Indisponível.
/// </summary>
public sealed record QuotationListItemResponse(
    Guid QuotationId,
    Guid InsurerId,
    string InsurerName,
    string? InsurerLogoUrl,
    string ProcessingStatus,
    string? Result,
    string? AnalysisTrack,
    bool IsFollowable,
    decimal? Premium,
    decimal? CommissionPercentage,
    decimal? CommissionValue,
    decimal? Tax,
    decimal? AvailableLimit,
    bool RequiresCcg,
    decimal? CcgMaxLimitWithoutNeed,
    bool CcgSigned,
    IReadOnlyList<string> Reasons);
