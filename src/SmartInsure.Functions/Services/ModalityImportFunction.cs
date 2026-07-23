using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SmartInsure.Application.UseCase.Services.ModalityImports;

namespace SmartInsure.Functions.Services;

/// <summary>
/// Importação agendada do ciclo de catálogo (RN-034): varre as Habilitações Ativas, importa pelo
/// Motor de Cálculo resolvido em cada vínculo e, no mesmo ciclo, importa a Tag e as Cláusulas
/// particulares de cada Modalidade Importada Ativa (RN-040/041/042). Falha por Corretora/Seguradora
/// é isolada (RN-038).
/// Cadência configurável por app setting <c>ModalityImport:Schedule</c> (OPEN-10, não crítico —
/// há default): commitado em <c>appsettings.json</c> deste projeto como <c>0 */30 * * * *</c>
/// (a cada 30 min, dev/QA). Produção deve sobrescrever via app setting/variável de ambiente do
/// Function App para <c>0 0 5 * * *</c> (1x/dia às 05:00) — nunca hardcodar cron por ambiente
/// no código. Atenção: a resolução de <c>%ModalityImport:Schedule%</c> é feita pelo host de
/// Functions via application settings (variável de ambiente/local.settings.json), não pelo
/// appsettings.json do worker isolado — infra deve garantir a variável de ambiente
/// <c>ModalityImport__Schedule</c> em todo ambiente que rodar este agendamento (ver report OPEN-10).
/// </summary>
public sealed class ModalityImportFunction(
    IModalityImporter modalityImporter,
    ILogger<ModalityImportFunction> logger)
{
    [Function(nameof(ModalityImportFunction))]
    public async Task RunAsync(
        [TimerTrigger("%ModalityImport:Schedule%")] TimerInfo timer, CancellationToken cancellationToken)
    {
        var summary = await modalityImporter.RunAsync(DateTime.UtcNow, cancellationToken);

        logger.LogInformation(
            "Importação de modalidades: {Processed} processadas, {Succeeded} com sucesso, {Failed} com falha.",
            summary.InsurersProcessed,
            summary.InsurersSucceeded,
            summary.InsurersFailed);

        foreach (var failure in summary.Failures)
        {
            logger.LogWarning("Falha na importação de modalidades: {Failure}", failure);
        }
    }
}
