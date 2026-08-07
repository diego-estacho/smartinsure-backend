namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationDetail.Requests;

/// <summary>
/// RN-081: entrada do detalhe read-only da Cotação. A identidade é o id (guid) — nunca o número
/// (<c>ProposalNumber</c> pode faltar e é só exibição). A Corretora ativa vem do acesso (RN-064),
/// resolvida no endpoint, nunca informada pelo cliente.
/// </summary>
public sealed record GetQuotationDetailRequest(Guid QuotationId, Guid? ActiveBrokerageId);
