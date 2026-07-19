namespace SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.UpdateInsurerEnablement.Responses;

/// <summary>Dados de saída da alteração da Habilitação de Seguradora.</summary>
public sealed record UpdateInsurerEnablementResponse(
    Guid Id,
    Guid BrokerageId,
    Guid InsurerId,
    string CalculationEngine,
    string? ConnectionParameters,
    string Status);
