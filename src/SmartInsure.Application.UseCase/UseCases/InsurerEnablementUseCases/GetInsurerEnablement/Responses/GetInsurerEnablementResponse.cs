namespace SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.GetInsurerEnablement.Responses;

/// <summary>Detalhe da Habilitação de Seguradora.</summary>
public sealed record GetInsurerEnablementResponse(
    Guid Id,
    Guid BrokerageId,
    string BrokerageName,
    Guid InsurerId,
    string InsurerCorporateName,
    string CalculationEngine,
    string? ConnectionParameters,
    string Status);
