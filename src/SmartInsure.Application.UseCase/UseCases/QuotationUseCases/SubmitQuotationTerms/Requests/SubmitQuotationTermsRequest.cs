namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SubmitQuotationTerms.Requests;

/// <summary>
/// RN-063 — "Baixar minuta": envia ao provedor os termos preenchidos (Tags do objeto + Cláusulas
/// particulares marcadas) da Cotação e devolve a minuta gerada. BrokerageId identifica a Corretora dona
/// da Habilitação (fonte dos parâmetros de conexão/motor — OPEN-03/RN-023). Preenchimento parcial é aceito.
/// </summary>
public sealed record SubmitQuotationTermsRequest(
    Guid QuotationId,
    Guid BrokerageId,
    IReadOnlyList<QuotationTermInput> Terms,
    IReadOnlyList<QuotationClauseInput> ParticularClauses);

/// <summary>Tag do objeto (ou de uma cláusula) preenchida pelo corretor: nome + valor (RN-062).</summary>
public sealed record QuotationTermInput(string Name, string Value);

/// <summary>
/// Cláusula particular marcada — identificada pelo id externo do catálogo importado (o mesmo devolvido
/// pela leitura da minuta, RN-062) — com suas Tags preenchidas.
/// </summary>
public sealed record QuotationClauseInput(string ParticularClauseExternalId, IReadOnlyList<QuotationTermInput> Tags);
