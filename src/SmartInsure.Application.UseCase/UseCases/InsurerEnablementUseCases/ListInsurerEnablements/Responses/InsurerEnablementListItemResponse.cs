namespace SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.ListInsurerEnablements.Responses;

/// <summary>Item da listagem de Habilitações de Seguradora.</summary>
public sealed record InsurerEnablementListItemResponse(
    Guid Id,
    Guid BrokerageId,
    string BrokerageName,
    Guid InsurerId,
    string InsurerCorporateName,
    string CalculationEngine,
    string Status);
