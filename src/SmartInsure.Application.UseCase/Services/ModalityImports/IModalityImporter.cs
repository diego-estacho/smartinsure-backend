namespace SmartInsure.Application.UseCase.Services.ModalityImports;

/// <summary>Serviço de importação de modalidades (RN-034); orquestrado pelo timer das Functions.</summary>
public interface IModalityImporter
{
    Task<ModalityImportSummary> RunAsync(DateTime nowUtc, CancellationToken cancellationToken);
}

/// <summary>Resumo da execução da importação (rastreio, RN-038).</summary>
public sealed record ModalityImportSummary(
    int InsurersProcessed,
    int InsurersSucceeded,
    int InsurersFailed,
    IReadOnlyList<string> Failures);
