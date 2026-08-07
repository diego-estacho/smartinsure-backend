namespace SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.RequestPolicyIssuance.Requests;

/// <summary>
/// RN-500: pedido de emissão da Cotação escolhida do Grupo. O parcelamento e o vencimento têm de constar
/// entre os informados pela Seguradora (RN-505), e o aceite do Termo é obrigatório (RN-506).
/// </summary>
public sealed record RequestPolicyIssuanceRequest
{
    public required Guid QuotationGroupId { get; init; }

    public required int InstallmentNumber { get; init; }

    public required int GracePeriodInDays { get; init; }

    /// <summary>RN-506: aceite explícito do Termo e declaração — nunca assumido como verdadeiro.</summary>
    public required bool TermAccepted { get; init; }

    /// <summary>Agente de acesso informado pela borda; parte da prova do aceite (RN-506).</summary>
    public string? UserAgent { get; init; }
}
