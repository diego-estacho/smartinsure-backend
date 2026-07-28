namespace SmartInsure.Core.Abstractions.Repositories.Dtos;

/// <summary>
/// Contexto de uma solicitação de cotação (RN-056): os dados compartilhados do Grupo + Corretora
/// necessários para montar a chamada ao motor por Seguradora. Projeção de leitura (ADR-032),
/// reconstruída pelo consumidor a partir do estado persistido (reconciliador-safe, ADR-050).
/// </summary>
public sealed record QuotationContextDto(
    string BrokerCnpj,
    string PolicyHolderCnpj,
    string InsuredCpfCnpj,
    string? ModalityGlobalId,
    string ModalityName,
    decimal InsuredAmount,
    DateOnly CoverageStartDate,
    DateOnly CoverageEndDate,
    bool IncludesPenaltyCoverage,
    bool IncludesLaborCoverage);
