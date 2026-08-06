namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationDetail.Responses;

/// <summary>
/// RN-081: detalhe read-only de uma Cotação. O resultado sai pelo **nome estável** (ADR-031); o rótulo
/// da situação apresentada (RN-078) é montado na apresentação. Número vazio quando a Seguradora não
/// informou; prêmio/comissão só quando aplicáveis (RN-058); a comissão em valor é a persistida (nunca
/// recalculada). O documento do Tomador/Segurado vai em dígitos crus (a máscara é apresentação).
/// </summary>
public sealed record QuotationDetailResponse(
    Guid QuotationId,
    string? Number,
    string PolicyHolderName,
    string PolicyHolderDocumentNumber,
    string InsuredName,
    string InsuredDocumentNumber,
    Guid InsurerId,
    string InsurerName,
    string? InsurerLogoUrl,
    Guid ModalityId,
    string ModalityName,
    decimal InsuredAmount,
    decimal? Premium,
    decimal? CommissionPercentage,
    decimal? CommissionValue,
    DateOnly CoverageStartDate,
    DateOnly CoverageEndDate,
    DateTime CreatedAt,
    string Result,
    bool RequiresCcg,
    bool CcgSigned,
    IReadOnlyList<QuotationDetailCoverageResponse> AdditionalCoverages,
    IReadOnlyList<QuotationTimelineEventResponse> Timeline);

/// <summary>
/// RN-106: uma Cobertura Adicional escolhida — nome canônico (exibível mesmo quando não contemplada), a
/// situação por **nome estável** e o nome enviado à Seguradora quando houve.
/// </summary>
public sealed record QuotationDetailCoverageResponse(string Name, string Status, string? SentName);

/// <summary>
/// RN-081: um marco da cronologia por **nome estável** (<see cref="QuotationTimelineEventTypes"/>) — o
/// rótulo/ícone pt-BR é montado na apresentação. Ordem: mais recente primeiro.
/// </summary>
public sealed record QuotationTimelineEventResponse(string Type, DateTime OccurredAt);

/// <summary>
/// RN-081: nomes estáveis dos marcos da cronologia que a plataforma conhece do pedido. Nada além destes é
/// inventado; o log durável nasce quando anexo/mensagem/cancelamento/emissão passarem a gerar eventos.
/// </summary>
public static class QuotationTimelineEventTypes
{
    public const string Created = "QuotationCreated";
    public const string Obtained = "QuotationObtained";
    public const string CcgRequired = "CcgRequired";
}
