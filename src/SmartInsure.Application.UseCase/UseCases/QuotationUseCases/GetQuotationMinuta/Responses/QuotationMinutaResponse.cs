namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.GetQuotationMinuta.Responses;

/// <summary>
/// RN-062: a minuta oferecida pela Seguradora da Cotação selecionada — as Tags do objeto (desenho do
/// formulário em JSON) e as Cláusulas particulares ativas. Vazia quando a Seguradora/Modalidade não
/// tem catálogo importado.
/// </summary>
public sealed record QuotationMinutaResponse(
    string? TagJson,
    IReadOnlyList<QuotationMinutaClauseResponse> Clauses);

/// <summary>RN-062: cláusula particular disponível (nome, texto e o desenho das tags próprias, se houver).</summary>
public sealed record QuotationMinutaClauseResponse(
    string ExternalId,
    string Name,
    string? ClauseText,
    string? JsonTag);
