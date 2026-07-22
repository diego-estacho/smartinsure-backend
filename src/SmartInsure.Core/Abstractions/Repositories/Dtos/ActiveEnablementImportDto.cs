namespace SmartInsure.Core.Abstractions.Repositories.Dtos;

/// <summary>
/// Habilitação Ativa para a importação (RN-031): CNPJ da Corretora, Seguradora (com a Referência
/// de origem para casar o retorno do motor) e os parâmetros de conexão do par.
/// </summary>
public sealed record ActiveEnablementImportDto(
    Guid EnablementId,
    Guid BrokerageId,
    string BrokerCnpj,
    Guid InsurerId,
    string? InsurerReferenceExternalId,
    string CalculationEngine,
    string? ConnectionParameters);
