namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SubmitQuotationTerms.Responses;

/// <summary>
/// RN-080: a minuta (documento) da proposta após o envio dos termos — normalmente uma URL para o
/// contrato gerado pelo provedor, mais o id e a data de geração. Campos nulos quando o provedor não
/// devolve a minuta (falha parcial não descarta o preenchimento local — tratado no front).
/// </summary>
public sealed record SubmitQuotationTermsResponse(string? DraftUrl, string? DraftExternalId, DateTime? DraftCreatedAt);
