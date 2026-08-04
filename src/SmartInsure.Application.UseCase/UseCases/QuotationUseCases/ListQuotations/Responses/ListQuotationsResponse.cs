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
    IReadOnlyList<string> Reasons,
    /// <summary>RN-505: opções de parcelamento informadas pela Seguradora nesta Cotação.</summary>
    IReadOnlyList<QuotationInstallmentOptionResponse> InstallmentOptions,
    /// <summary>RN-505: dias possíveis para o vencimento da primeira parcela.</summary>
    IReadOnlyList<int> PossibleGracePeriodsInDays,
    /// <summary>RN-510: documentos que a Seguradora exige para emitir; informativos ao corretor.</summary>
    IReadOnlyList<QuotationRequiredDocumentResponse> RequiredDocuments);

/// <summary>
/// RN-505: opção de parcelamento oferecida pela Seguradora. A etapa de emissão escolhe **dentro** desta
/// lista — a plataforma não calcula parcela nem oferece opção própria.
/// </summary>
public sealed record QuotationInstallmentOptionResponse(
    int Number,
    string? Description,
    decimal Value,
    bool HasInterest);

/// <summary>RN-510: documento exigido pela Seguradora para emitir.</summary>
public sealed record QuotationRequiredDocumentResponse(string Name, string? Description);
