namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.RunQuotations.Requests;

/// <summary>RN-056: dados de entrada para solicitar as Cotações de um Grupo de Cotação.</summary>
/// <param name="QuotationGroupId">Grupo de Cotação (em Rascunho) a cotar.</param>
/// <remarks>
/// RN-103: a Corretora da solicitação é a do Escopo ativo do acesso (RN-064, ADR-065), resolvida pelo
/// servidor a partir do claim — nunca informada pelo cliente. Por isso não há Corretora neste request.
/// </remarks>
public sealed record RunQuotationsRequest(
    Guid QuotationGroupId);
