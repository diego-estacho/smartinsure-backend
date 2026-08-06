namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationMinuta.Requests;

/// <summary>
/// RN-079: identifica a Cotação selecionada cuja minuta (Tags + Cláusulas) será exibida. O
/// <see cref="QuotationGroupId"/> (da rota) confere que a Cotação pertence mesmo a este Grupo.
/// </summary>
public sealed record GetQuotationMinutaRequest(Guid QuotationGroupId, Guid QuotationId);
