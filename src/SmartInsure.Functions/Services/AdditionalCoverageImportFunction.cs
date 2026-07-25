using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SmartInsure.Application.UseCase.Services.AdditionalCoverageImports;

namespace SmartInsure.Functions.Services;

/// <summary>
/// Importação agendada de Coberturas Adicionais (RN-044): varre as Habilitações Ativas e, por
/// Modalidade Importada processável, consulta a OnPoint pelo Motor de Cálculo resolvido no vínculo.
/// Falha por modalidade é isolada (RN-046). Cadência configurável por app setting
/// `AdditionalCoverageImportSchedule` (RN-044/OPEN-10); default a cada 30min, prod ajusta para
/// 1x/dia às 05:00 (`0 0 5 * * *`).
/// </summary>
public sealed class AdditionalCoverageImportFunction(
    IAdditionalCoverageImporter importer,
    ILogger<AdditionalCoverageImportFunction> logger)
{
    [Function(nameof(AdditionalCoverageImportFunction))]
    public async Task RunAsync(
        [TimerTrigger("%AdditionalCoverageImportSchedule%")] TimerInfo timer, CancellationToken cancellationToken)
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
