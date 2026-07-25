namespace SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.UpdateBrokerage.Requests;

/// <summary>RN-034 — edição de dados complementares da Corretora (nome fantasia e contato).</summary>
public sealed record UpdateBrokerageRequest(
    Guid BrokerageId,
    string? SocialName,
    string? ContactEmail,
    string? ContactPhone,
    string? ResponsibleName);
