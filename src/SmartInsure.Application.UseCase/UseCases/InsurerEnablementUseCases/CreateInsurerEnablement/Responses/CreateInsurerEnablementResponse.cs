namespace SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.CreateInsurerEnablement.Responses;

/// <summary>Dados de saída da Habilitação de Seguradora criada.</summary>
public sealed record CreateInsurerEnablementResponse(
    Guid Id,
    Guid BrokerageId,
    Guid InsurerId,
    string CalculationEngine,
    string? ConnectionParameters,
    string Status);
