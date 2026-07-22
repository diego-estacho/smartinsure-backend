using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SmartInsure.Application.UseCase.Services.ModalityImports;

namespace SmartInsure.Functions.Services;

/// <summary>
/// Importação agendada de modalidades (RN-031): varre as Habilitações Ativas e importa pelo
/// Motor de Cálculo resolvido em cada vínculo. Falha por Corretora/Seguradora é isolada (RN-035).
/// Cadência default diária às 03:00 UTC — OPEN-10 (tornar configurável por app setting).
/// </summary>
public sealed class ModalityImportFunction(
    IModalityImporter modalityImporter,
    ILogger<ModalityImportFunction> logger)
{
    [Function(nameof(ModalityImportFunction))]
    public async Task RunAsync(
        [TimerTrigger("0 0 3 * * *")] TimerInfo timer, CancellationToken cancellationToken)
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
