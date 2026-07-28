namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationsStatus.Responses;

/// <summary>
/// Estado do fan-out de um Grupo (RN-057, ADR-051): progresso + as Cotações já obtidas.
/// Leitura barata do estado persistido — o polling reflete o preenchimento progressivo.
/// </summary>
public sealed record QuotationsStatusResponse(
    Guid QuotationGroupId,
    Guid? SelectedQuotationId,
    int Total,
    int Obtained,
    int Failed,
    int Pending,
    bool Completed,
    IReadOnlyList<QuotationItemResponse> Quotations);

/// <summary>Uma Cotação na tela (RN-058): classificação + esteira específica + motivos + CCG.</summary>
public sealed record QuotationItemResponse(
    Guid Id,
    Guid InsurerId,
    string ProcessingStatus,
    string? Result,
    string? AnalysisTrack,
    decimal? Premium,
    decimal? CommissionPercentage,
    decimal? CommissionValue,
    decimal? Tax,
    decimal? AvailableLimit,
    bool RequiresCcg,
    decimal? CcgMaxLimitWithoutNeed,
    bool CcgSigned,
    bool IsFollowable,
    DateTime? ObtainedAt,
    IReadOnlyList<string> Reasons);
