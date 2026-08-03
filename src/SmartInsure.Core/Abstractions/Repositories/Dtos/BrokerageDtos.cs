using SmartInsure.Core.Enumerators;

namespace SmartInsure.Core.Abstractions.Repositories.Dtos;

/// <summary>RN-018 — filtros server-side da listagem de Corretoras (a base pode passar de 10 mil).</summary>
public sealed record BrokerageListQuery(
    int Page,
    int PageSize,
    string? Search,
    EBrokerageSituation? Situation,
    Guid? InsurerId,
    ECalculationEngine? CalculationEngine,
    bool? IsPrivateSector,
    DateTime? RegisteredFrom,
    DateTime? RegisteredTo);

public sealed record BrokerageListResult(
    IReadOnlyList<BrokerageListItemDto> Items,
    long TotalCount,
    BrokerageSituationCountsDto Counts);

/// <summary>RN-018/RN-102 — contagem por situação apresentada, considerando os demais filtros.</summary>
public sealed record BrokerageSituationCountsDto(
    long All,
    long Active,
    long Incomplete,
    long Inactive);

public sealed record BrokerageListItemDto(
    Guid Id,
    string DocumentNumber,
    string Name,
    string? SocialName,
    bool? IsPrivateSector,
    string Status,
    string Situation,
    DateTime RegisteredAt,
    int EnabledInsurerCount,
    IReadOnlyList<string> EnabledInsurerNames,
    IReadOnlyList<string> CalculationEngines);

public sealed record BrokerageDetailsDto(
    Guid Id,
    string DocumentNumber,
    string Name,
    string? SocialName,
    string? LegalNatureCode,
    string? LegalNatureDescription,
    bool? IsPrivateSector,
    string Status,
    string Situation,
    string? ContactEmail,
    string? ContactPhone,
    string? ResponsibleName,
    DateTime RegisteredAt,
    int EnabledInsurerCount,
    PersonMainAddressDto? MainAddress);

/// <summary>RN-055 — evento da linha do tempo da Corretora, derivado da auditoria (sem tabela de eventos).</summary>
public sealed record BrokerageHistoryEventDto(
    string Type,
    string? Subject,
    DateTime OccurredAt,
    string Author);

/// <summary>RN-101 — dados de um CNPJ já cadastrado, para a consulta somente leitura do cadastro.</summary>
public sealed record BrokeragePreviewDto(
    Guid PersonId,
    string DocumentNumber,
    string Name,
    string? SocialName,
    string? LegalNatureCode,
    string? LegalNatureName,
    bool? IsPrivateSector,
    bool HasBrokerRole,
    PersonMainAddressDto? MainAddress,
    DateTime LastUpdatedAt);
