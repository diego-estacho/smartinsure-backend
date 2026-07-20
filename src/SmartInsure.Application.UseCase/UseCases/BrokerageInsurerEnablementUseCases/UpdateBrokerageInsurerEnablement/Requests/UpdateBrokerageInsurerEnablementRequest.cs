namespace SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.UpdateBrokerageInsurerEnablement.Requests;

/// <summary>Alteração do motor e dos parâmetros de conexão da Habilitação (RN-022); o par não muda.</summary>
public sealed record UpdateBrokerageInsurerEnablementRequest(
    Guid Id,
    string CalculationEngine,
    string? ConnectionParameters);
