namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SelectQuotation.Responses;

/// <summary>RN-059: a Cotação escolhida do Grupo.</summary>
public sealed record SelectQuotationResponse(
    Guid QuotationGroupId,
    Guid SelectedQuotationId);
