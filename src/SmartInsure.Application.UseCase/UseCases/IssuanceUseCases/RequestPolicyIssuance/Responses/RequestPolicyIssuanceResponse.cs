namespace SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.RequestPolicyIssuance.Responses;

/// <summary>
/// RN-514: confirmação de que a emissão foi **solicitada**. A oferta é identificada pelo número da
/// proposta devolvido pela Seguradora; número da apólice, arquivo e boletos vêm da confirmação, que é
/// demanda própria — a plataforma não afirma o que não confirmou.
/// </summary>
public sealed record RequestPolicyIssuanceResponse
{
    public required Guid PolicyId { get; init; }

    public required string PolicyExternalId { get; init; }

    public string? ProposalNumber { get; init; }

    public required DateTime RequestedAt { get; init; }

    /// <summary>Situação da oferta após o pedido — "Emissão solicitada" (RN-508).</summary>
    public required string QuotationGroupStatus { get; init; }
}
