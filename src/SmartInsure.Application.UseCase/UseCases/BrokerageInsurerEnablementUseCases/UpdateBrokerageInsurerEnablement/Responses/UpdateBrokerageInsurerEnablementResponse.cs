namespace SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.UpdateBrokerageInsurerEnablement.Responses;

/// <summary>Dados de saída da alteração da Habilitação de Seguradora.</summary>
public sealed record UpdateBrokerageInsurerEnablementResponse(
    Guid Id,
    Guid BrokerageId,
    Guid InsurerId,
    string CalculationEngine,
    string? ConnectionParameters,
    string Status);
