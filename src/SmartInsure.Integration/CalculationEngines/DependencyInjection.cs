using Microsoft.Extensions.DependencyInjection;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Enumerators;
using SmartInsure.Integration.CalculationEngines.PlugV2;
using SmartInsure.Integration.CalculationEngines.Services;

namespace SmartInsure.Integration.CalculationEngines;

public static class DependencyInjection
{
    public static IServiceCollection AddCalculationEngines(this IServiceCollection services)
    {
        // Configuração de resiliência por fornecedor (isolada, ajustável por ambiente) — nunca global.
        services.AddOptions<PlugV2Options>().BindConfiguration(PlugV2Options.SectionName);

        // Cliente PlugV2 para leituras VERDADEIRAMENTE idempotentes (minuta) — retry padrão é seguro,
        // repetir não gera efeito colateral no gateway. A consulta de LIMITES saiu daqui: embora seja
        // leitura, o gateway a dedupa como "consulta" (o retry cairia em "já existe") — ver o client
        // sem retry abaixo (plugv2-dedup).
        services.AddHttpClient("PlugV2")
            .AddStandardResilienceHandler();

        // RN-057: cliente PlugV2 SEM retry — para chamadas que o gateway dedupa: as mutantes (/Cotation,
        // /UpdateProposalTerms) E a consulta de limites (GetPolicyHolderLimitsAndRates). Re-tentar re-dispara
        // e cai no dedup do gateway ("já existe uma cotação/consulta"); a resiliência padrão re-tentaria no
        // timeout de tentativa (10s). Aqui: tentativa única, com timeout generoso e configurável (PlugV2Options).
        services.AddHttpClient(PlugV2CalculationEngine.NonIdempotentClientName);

        // RN-023: motores registrados por chave do enum — a escolha em runtime é sempre
        // da Habilitação de Seguradora, via ICalculationEngineResolver. A conexão
        // (baseUrl/key) é por vínculo (ConnectionParameters), não configuração global.
        services.AddKeyedScoped<ICalculationEngine, PlugV2CalculationEngine>(ECalculationEngine.PlugV2);

        services.AddScoped<ICalculationEngineResolver, CalculationEngineResolver>();

        // RN-034/ADR-044: base URL por Habilitação (montada por chamada), resiliência no client nomeado.
        services.AddHttpClient(PlugV2ModalityImportClient.HttpClientName)
            .AddStandardResilienceHandler();
        services.AddScoped<PlugV2ModalityImportClient>();

        // RN-047/ADR-044: idem para GetModalityObject — base URL por Habilitação, resiliência no client nomeado.
        services.AddHttpClient(PlugV2ModalityObjectClient.HttpClientName)
            .AddStandardResilienceHandler();
        services.AddScoped<PlugV2ModalityObjectClient>();

        // RN-042/RN-044/ADR-044: base URL por Habilitação (montada por chamada), resiliência no client nomeado.
        services.AddHttpClient(PlugV2AdditionalCoveragesClient.HttpClientName)
            .AddStandardResilienceHandler();
        services.AddScoped<PlugV2AdditionalCoveragesClient>();

        return services;
    }
}
