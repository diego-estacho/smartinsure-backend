namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SubmitQuotationTerms.Requests;

/// <summary>
/// RN-063 — "Baixar minuta": envia ao provedor os termos preenchidos (Tags do objeto + Cláusulas
/// particulares marcadas) da Cotação e devolve a minuta gerada. Preenchimento parcial é aceito.
/// </summary>
/// <remarks>
/// RN-103: a Corretora do envio (dona da Habilitação, fonte da conexão/motor — RN-023) é a do Escopo ativo
/// do acesso (RN-064, ADR-065), resolvida pelo servidor a partir do claim — nunca informada pelo cliente.
/// </remarks>
public sealed record SubmitQuotationTermsRequest(
    Guid QuotationGroupId,
    Guid QuotationId,
    IReadOnlyList<QuotationTermInput> Terms,
    IReadOnlyList<QuotationClauseInput> ParticularClauses);

/// <summary>Tag do objeto (ou de uma cláusula) preenchida pelo corretor: nome + valor (RN-062).</summary>
public sealed record QuotationTermInput(string Name, string Value);

/// <summary>
/// Cláusula particular marcada — identificada pelo id externo do catálogo importado (o mesmo devolvido
/// pela leitura da minuta, RN-062) — com suas Tags preenchidas.
/// </summary>
public sealed record QuotationClauseInput(string ParticularClauseExternalId, IReadOnlyList<QuotationTermInput> Tags);
