using SmartInsure.Application.UseCase.Services.ModalityImports;
using SmartInsure.Application.UseCase.UseCases.ModalityImportUseCases.RunModalityImport.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ModalityImportUseCases.RunModalityImport.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityImportUseCases.RunModalityImport.Responses;

namespace SmartInsure.Application.UseCase.UseCases.ModalityImportUseCases.RunModalityImport;

/// <summary>RN-031 — disparo manual da importação (operação/teste); o agendado roda pelo timer das Functions.</summary>
public sealed class RunModalityImportUseCase(IModalityImporter modalityImporter) : IRunModalityImportUseCase
{
    public async Task<ModalityImportSummaryResponse> ExecuteAsync(
        RunModalityImportRequest request, CancellationToken cancellationToken)
    {
        var summary = await modalityImporter.RunAsync(DateTime.UtcNow, cancellationToken);

        return new ModalityImportSummaryResponse(
            summary.InsurersProcessed, summary.InsurersSucceeded, summary.InsurersFailed, summary.Failures);
    }
}
