using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Integration.CalculationEngines.Services;

/// <summary>
/// Motor de Cálculo PlugV2 (RN-023): único motor disponível nesta fase. As operações do
/// contrato (cotar, prêmio, dados de apoio, emissão, cancelamento) entram nas demandas
/// de cada jornada (OPEN-07), consumindo o gateway configurado em PlugV2Options com os
/// parâmetros de conexão da Habilitação resolvida.
/// </summary>
public sealed class PlugV2CalculationEngine : ICalculationEngine
{
    public ECalculationEngine Engine => ECalculationEngine.PlugV2;
}
