namespace SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.ChangeBrokerageInsurerEnablementStatus.Requests;

/// <summary>Alternância de situação da Habilitação (RN-022), pelo nome estável Active/Inactive.</summary>
public sealed record ChangeBrokerageInsurerEnablementStatusRequest(Guid Id, string Status);
