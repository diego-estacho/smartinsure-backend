namespace SmartInsure.Core.Abstractions.Repositories.Dtos;

public sealed record InsurerEnablementListItemDto(
    Guid Id,
    Guid BrokerageId,
    string BrokerageName,
    Guid InsurerId,
    string InsurerCorporateName,
    string CalculationEngine,
    string Status);

public sealed record InsurerEnablementDetailsDto(
    Guid Id,
    Guid BrokerageId,
    string BrokerageName,
    Guid InsurerId,
    string InsurerCorporateName,
    string CalculationEngine,
    string? ConnectionParameters,
    string Status);
