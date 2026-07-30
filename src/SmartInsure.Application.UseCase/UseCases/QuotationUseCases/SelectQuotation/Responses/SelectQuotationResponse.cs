namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SelectQuotation.Responses;

/// <summary>RN-059: a escolha registrada — o Grupo e a Cotação marcada.</summary>
public sealed record SelectQuotationResponse(
    Guid QuotationGroupId,
    Guid SelectedQuotationId);
