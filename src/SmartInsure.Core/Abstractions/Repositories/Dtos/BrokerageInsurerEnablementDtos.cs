namespace SmartInsure.Core.Abstractions.Repositories.Dtos;

public sealed record BrokerageInsurerEnablementListItemDto(
    Guid Id,
    Guid BrokerageId,
    string BrokerageName,
    Guid InsurerId,
    string InsurerCorporateName,
    string CalculationEngine,
    string Status);

public sealed record BrokerageInsurerEnablementDetailsDto(
    Guid Id,
    Guid BrokerageId,
    string BrokerageName,
    Guid InsurerId,
    string InsurerCorporateName,
    string CalculationEngine,
    string? ConnectionParameters,
    string Status);
