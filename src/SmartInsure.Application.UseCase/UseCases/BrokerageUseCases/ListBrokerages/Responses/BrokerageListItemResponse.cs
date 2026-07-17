namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ListBrokerages.Responses;

public sealed record BrokerageListItemResponse(
    Guid Id,
    string DocumentNumber,
    string Name,
    string? SocialName,
    bool? IsPrivateSector,
    string Status);
