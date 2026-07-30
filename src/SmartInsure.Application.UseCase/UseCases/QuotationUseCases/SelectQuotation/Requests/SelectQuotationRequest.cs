namespace SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SelectQuotation.Requests;

/// <summary>RN-059: dados de entrada para marcar a Cotação escolhida de um Grupo de Cotação.</summary>
/// <param name="QuotationGroupId">Identificador do Grupo de Cotação.</param>
/// <param name="QuotationId">Identificador da Cotação a escolher (deve ser seguível e do Grupo).</param>
public sealed record SelectQuotationRequest(
    Guid QuotationGroupId,
    Guid QuotationId);
