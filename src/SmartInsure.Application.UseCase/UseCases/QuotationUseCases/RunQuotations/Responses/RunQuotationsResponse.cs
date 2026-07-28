namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.RunQuotations.Responses;

/// <summary>RN-057: aceite do disparo. O acompanhamento é por polling do GET de status (ADR-051).</summary>
/// <param name="QuotationGroupId">Grupo cotado.</param>
/// <param name="RequestedCount">Quantidade de Cotações solicitadas (uma por Seguradora do escopo).</param>
public sealed record RunQuotationsResponse(
    Guid QuotationGroupId,
    int RequestedCount);
