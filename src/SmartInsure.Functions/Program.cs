using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using SmartInsure.Application.UseCase.IoC;
using SmartInsure.Infra.Data;
using SmartInsure.Integration.CalculationEngines;

var builder = FunctionsApplication.CreateBuilder(args);

// Composição da DI do job de importação de modalidades (RN-034): dados (SQL Server),
// casos de uso/serviço da Application e motores de cálculo (PlugV2). Não compõe Casdoor/Bureau/
// Mail/JWT — o job não os usa; a conexão do PlugV2 vem da Habilitação (ConnectionParameters).
builder.Services.AddInfraData(builder.Configuration);
builder.Services.AddApplicationUseCases();
builder.Services.AddCalculationEngines();

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

builder.Build().Run();
