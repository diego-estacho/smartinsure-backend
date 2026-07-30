namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotations.Requests;

/// <summary>RN-057: identifica o Grupo cujas Cotações (o leque) serão lidas — usado no acompanhamento (polling).</summary>
public sealed record ListQuotationsRequest(Guid QuotationGroupId);
