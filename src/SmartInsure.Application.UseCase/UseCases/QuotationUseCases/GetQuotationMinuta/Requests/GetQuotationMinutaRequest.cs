namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationMinuta.Requests;

/// <summary>RN-062: identifica a Cotação selecionada cuja minuta (Tags + Cláusulas) será exibida.</summary>
public sealed record GetQuotationMinutaRequest(Guid QuotationId);
