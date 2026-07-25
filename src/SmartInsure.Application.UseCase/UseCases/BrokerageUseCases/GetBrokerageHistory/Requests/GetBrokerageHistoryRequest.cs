namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.GetBrokerageHistory.Requests;

/// <summary>RN-035 — linha do tempo de uma Corretora.</summary>
public sealed record GetBrokerageHistoryRequest(Guid BrokerageId);
