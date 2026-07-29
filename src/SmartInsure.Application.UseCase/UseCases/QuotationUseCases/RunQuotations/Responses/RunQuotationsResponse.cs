namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.RunQuotations.Responses;

/// <summary>RN-056/RN-057: a solicitação disparada — o Grupo e quantas Cotações foram enfileiradas.</summary>
public sealed record RunQuotationsResponse(
    Guid QuotationGroupId,
    int RequestedCount);
