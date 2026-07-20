namespace SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.GetBrokerageInsurerEnablement.Responses;

/// <summary>Detalhe da Habilitação de Seguradora.</summary>
public sealed record GetBrokerageInsurerEnablementResponse(
    Guid Id,
    Guid BrokerageId,
    string BrokerageName,
    Guid InsurerId,
    string InsurerCorporateName,
    string CalculationEngine,
    string? ConnectionParameters,
    string Status);
