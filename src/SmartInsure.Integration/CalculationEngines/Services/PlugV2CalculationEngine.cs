using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Enumerators;
using SmartInsure.Integration.CalculationEngines.PlugV2;

namespace SmartInsure.Integration.CalculationEngines.Services;

/// <summary>
/// Motor de Cálculo PlugV2 (RN-023): único motor disponível nesta fase. As operações do
/// contrato (cotar, prêmio, dados de apoio, emissão, cancelamento) entram nas demandas
/// de cada jornada (OPEN-07), consumindo o gateway com os parâmetros de conexão da
/// Habilitação resolvida (baseUrl/key), o CNPJ da Corretora do vínculo e o
/// ReferenceExternalId da Seguradora.
/// </summary>
public sealed class PlugV2CalculationEngine : ICalculationEngine
{
    public ECalculationEngine Engine => ECalculationEngine.PlugV2;

    public void EnsureValidConnectionParameters(string? connectionParameters)
        => PlugV2ConnectionParameters.Parse(connectionParameters);
}
