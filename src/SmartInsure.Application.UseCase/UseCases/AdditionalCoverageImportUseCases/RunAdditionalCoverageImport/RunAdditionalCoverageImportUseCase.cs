using SmartInsure.Application.UseCase.Services.AdditionalCoverageImports;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageImportUseCases.RunAdditionalCoverageImport.Interfaces;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageImportUseCases.RunAdditionalCoverageImport.Requests;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageImportUseCases.RunAdditionalCoverageImport.Responses;

namespace SmartInsure.Application.UseCase.UseCases.AdditionalCoverageImportUseCases.RunAdditionalCoverageImport;

/// <summary>RN-044 — disparo sob demanda da importação de Coberturas Adicionais (Administrador do Sistema); o agendado roda pelo timer das Functions.</summary>
public sealed class RunAdditionalCoverageImportUseCase(IAdditionalCoverageImporter importer)
    : IRunAdditionalCoverageImportUseCase
{
    public async Task<AdditionalCoverageImportSummaryResponse> ExecuteAsync(
        RunAdditionalCoverageImportRequest request, CancellationToken cancellationToken)
    {
        var summary = await importer.RunAsync(DateTime.UtcNow, cancellationToken);

        return new AdditionalCoverageImportSummaryResponse(
            summary.ModalitiesProcessed, summary.ModalitiesSucceeded, summary.ModalitiesFailed, summary.Failures);
    }
}
