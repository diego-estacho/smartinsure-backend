namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SelectQuotation.Requests;

/// <summary>RN-059: escolhe uma Cotação seguível do Grupo para seguir.</summary>
public sealed record SelectQuotationRequest(
    Guid QuotationGroupId,
    Guid QuotationId);
