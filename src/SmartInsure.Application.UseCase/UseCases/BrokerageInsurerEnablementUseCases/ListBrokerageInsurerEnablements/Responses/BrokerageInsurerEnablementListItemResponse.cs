namespace SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.ListBrokerageInsurerEnablements.Responses;

/// <summary>Item da listagem de Habilitações de Seguradora.</summary>
public sealed record BrokerageInsurerEnablementListItemResponse(
    Guid Id,
    Guid BrokerageId,
    string BrokerageName,
    Guid InsurerId,
    string InsurerCorporateName,
    string? InsurerLogoUrl,
    string CalculationEngine,
    string Status);
