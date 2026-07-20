namespace SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.CreateBrokerageInsurerEnablement.Responses;

/// <summary>Dados de saída da Habilitação de Seguradora criada.</summary>
public sealed record CreateBrokerageInsurerEnablementResponse(
    Guid Id,
    Guid BrokerageId,
    Guid InsurerId,
    string CalculationEngine,
    string? ConnectionParameters,
    string Status);
