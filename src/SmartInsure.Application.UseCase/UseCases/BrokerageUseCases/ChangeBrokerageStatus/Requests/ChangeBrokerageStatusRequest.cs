namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ChangeBrokerageStatus.Requests;

public sealed record ChangeBrokerageStatusRequest(Guid BrokerageId, string Status);
