namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.RunQuotations.Requests;

/// <summary>RN-056: dados de entrada para solicitar as Cotações de um Grupo de Cotação.</summary>
/// <param name="QuotationGroupId">Grupo de Cotação (em Rascunho) a cotar.</param>
/// <param name="BrokerageId">Corretora dona das Habilitações de Seguradora (fonte OPEN-03).</param>
public sealed record RunQuotationsRequest(
    Guid QuotationGroupId,
    Guid BrokerageId);
