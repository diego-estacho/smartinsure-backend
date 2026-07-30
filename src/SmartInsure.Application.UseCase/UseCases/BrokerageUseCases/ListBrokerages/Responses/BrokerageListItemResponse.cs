namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ListBrokerages.Responses;

public sealed record BrokerageListItemResponse(
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
