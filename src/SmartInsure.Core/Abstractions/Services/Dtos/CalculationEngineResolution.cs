namespace SmartInsure.Core.Abstractions.Services.Dtos;

/// <summary>
/// Resultado da resolução do Motor de Cálculo (RN-023): a instância do motor e os
/// parâmetros de conexão registrados na Habilitação de Seguradora do par.
/// </summary>
public sealed record CalculationEngineResolution(
    ICalculationEngine Engine,
    string? ConnectionParameters);
