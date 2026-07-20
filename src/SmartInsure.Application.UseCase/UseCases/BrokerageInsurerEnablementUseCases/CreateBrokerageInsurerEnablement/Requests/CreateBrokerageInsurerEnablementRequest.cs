namespace SmartInsure.Application.UseCase.UseCases.BrokerageInsurerEnablementUseCases.CreateBrokerageInsurerEnablement.Requests;

/// <summary>Dados de entrada para habilitar uma Seguradora para a Corretora (RN-022).</summary>
/// <param name="BrokerageId">Identificador da Corretora (Pessoa com papel Corretor).</param>
/// <param name="InsurerId">Identificador da Seguradora no catálogo.</param>
/// <param name="CalculationEngine">Motor de Cálculo pelo nome estável (ex.: PlugV2).</param>
/// <param name="ConnectionParameters">Parâmetros de conexão exigidos pelo motor (opcional).</param>
public sealed record CreateBrokerageInsurerEnablementRequest(
    Guid BrokerageId,
    Guid InsurerId,
    string CalculationEngine,
    string? ConnectionParameters);
