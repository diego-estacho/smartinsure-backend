namespace SmartInsure.Application.UseCase.Services.AdditionalCoverageImports;

/// <summary>Serviço de importação de Coberturas Adicionais (RN-044); orquestrado pelo timer das Functions e pelo disparo sob demanda.</summary>
public interface IAdditionalCoverageImporter
{
    Task<AdditionalCoverageImportSummary> RunAsync(DateTime nowUtc, CancellationToken cancellationToken);
}

/// <summary>Resumo auditável da execução da importação, por recurso = Modalidade Importada consultada (RN-046).</summary>
public sealed record AdditionalCoverageImportSummary(
    int ModalitiesProcessed,
    int ModalitiesSucceeded,
    int ModalitiesFailed,
    IReadOnlyList<string> Failures);
