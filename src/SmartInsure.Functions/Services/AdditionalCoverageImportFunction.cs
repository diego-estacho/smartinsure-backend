using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SmartInsure.Application.UseCase.Services.AdditionalCoverageImports;

namespace SmartInsure.Functions.Services;

/// <summary>
/// Importação agendada de Coberturas Adicionais (RN-044): varre as Habilitações Ativas e, por
/// Modalidade Importada processável, consulta a OnPoint pelo Motor de Cálculo resolvido no vínculo.
/// Falha por modalidade é isolada (RN-046).
/// Cadência configurável por app setting <c>AdditionalCoverageImport:Schedule</c> (RN-044/OPEN-10,
/// não crítico — há default): commitado em <c>appsettings.json</c> deste projeto como
/// <c>0 */30 * * * *</c> (a cada 30 min, dev/QA). Produção deve sobrescrever via app setting/variável
/// de ambiente do Function App para <c>0 0 5 * * *</c> (1x/dia às 05:00) — nunca hardcodar cron por
/// ambiente no código. Atenção: a resolução de <c>%AdditionalCoverageImport:Schedule%</c> é feita pelo
/// host de Functions via application settings (variável de ambiente/local.settings.json), não pelo
/// appsettings.json do worker isolado — infra deve garantir a variável de ambiente
/// <c>AdditionalCoverageImport__Schedule</c> em todo ambiente que rodar este agendamento.
/// </summary>
public sealed class AdditionalCoverageImportFunction(
    IAdditionalCoverageImporter importer,
    ILogger<AdditionalCoverageImportFunction> logger)
{
    [Function(nameof(AdditionalCoverageImportFunction))]
    public async Task RunAsync(
        [TimerTrigger("%AdditionalCoverageImport:Schedule%")] TimerInfo timer, CancellationToken cancellationToken)
    {
        var summary = await importer.RunAsync(DateTime.UtcNow, cancellationToken);

        logger.LogInformation(
            "Importação de coberturas adicionais: {Processed} processadas, {Succeeded} com sucesso, {Failed} com falha.",
            summary.ModalitiesProcessed,
            summary.ModalitiesSucceeded,
            summary.ModalitiesFailed);

        foreach (var failure in summary.Failures)
        {
            logger.LogWarning("Falha na importação de coberturas adicionais: {Failure}", failure);
        }
    }
}
