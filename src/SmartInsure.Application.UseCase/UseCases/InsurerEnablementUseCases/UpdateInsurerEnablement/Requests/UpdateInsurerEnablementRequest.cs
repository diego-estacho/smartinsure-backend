namespace SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.UpdateInsurerEnablement.Requests;

/// <summary>Alteração do motor e dos parâmetros de conexão da Habilitação (RN-022); o par não muda.</summary>
public sealed record UpdateInsurerEnablementRequest(
    Guid Id,
    string CalculationEngine,
    string? ConnectionParameters);
