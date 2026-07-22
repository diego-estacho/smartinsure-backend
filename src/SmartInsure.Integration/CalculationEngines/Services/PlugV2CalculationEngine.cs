using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Enumerators;
using SmartInsure.Integration.CalculationEngines.PlugV2;

namespace SmartInsure.Integration.CalculationEngines.Services;

/// <summary>
/// Motor de Cálculo PlugV2 (RN-023): único motor disponível nesta fase. A importação de
/// modalidades (RN-031) consome o gateway com os parâmetros de conexão da Habilitação resolvida
/// (baseUrl/key) e o CNPJ da Corretora do vínculo; a tradução do payload fica na ACL (ADR-045).
/// As demais operações (cotar, prêmio, emissão, cancelamento) entram nas demandas de cada jornada (OPEN-07).
/// </summary>
public sealed class PlugV2CalculationEngine(PlugV2ModalityImportClient importClient) : ICalculationEngine
{
    public ECalculationEngine Engine => ECalculationEngine.PlugV2;

    public void EnsureValidConnectionParameters(string? connectionParameters)
        => PlugV2ConnectionParameters.Parse(connectionParameters);

    public Task<ImportedCatalogResult> GetGroupAndModalitiesAsync(
        string? connectionParameters, string brokerCnpj, CancellationToken cancellationToken)
    {
        var connection = PlugV2ConnectionParameters.Parse(connectionParameters);
        return importClient.GetGroupAndModalitiesAsync(connection, brokerCnpj, cancellationToken);
    }
}
