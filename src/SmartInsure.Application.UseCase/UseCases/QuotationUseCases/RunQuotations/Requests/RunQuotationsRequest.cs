namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.RunQuotations.Requests;

/// <summary>RN-056: solicita as Cotações de um Grupo às Seguradoras da Corretora.</summary>
/// <param name="QuotationGroupId">Grupo de Cotação (em Rascunho) a cotar.</param>
/// <param name="BrokerageId">Corretora que solicita (resolve as Habilitações e o CNPJ do broker).</param>
public sealed record RunQuotationsRequest(
    Guid QuotationGroupId,
    Guid BrokerageId);
