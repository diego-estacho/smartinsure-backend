namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.GetBrokerageHistory.Responses;

public sealed record GetBrokerageHistoryResponse(
    IReadOnlyList<BrokerageHistoryEventResponse> Events);

/// <summary>
/// RN-035 — evento com nome estável de tipo (created, insurer-enabled, insurer-enablement-updated,
/// data-updated); o texto de UI é composto no cliente. Subject traz a Seguradora nos eventos de habilitação.
/// </summary>
public sealed record BrokerageHistoryEventResponse(
    string Type,
    string? Subject,
    DateTime OccurredAt,
    string Author);
