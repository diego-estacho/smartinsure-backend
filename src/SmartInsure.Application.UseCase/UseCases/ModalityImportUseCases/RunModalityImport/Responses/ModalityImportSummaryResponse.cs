namespace SmartInsure.Application.UseCase.UseCases.ModalityImportUseCases.RunModalityImport.Responses;

/// <summary>Resumo da importação (RN-035): totais por Seguradora e motivos de falha.</summary>
public sealed record ModalityImportSummaryResponse(
    int InsurersProcessed,
    int InsurersSucceeded,
    int InsurersFailed,
    IReadOnlyList<string> Failures);
