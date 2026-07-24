namespace SmartInsure.Application.UseCase.UseCases.AdditionalCoverageImportUseCases.RunAdditionalCoverageImport.Responses;

/// <summary>Resumo da importação de Coberturas Adicionais (RN-046): totais por Modalidade Importada e motivos de falha.</summary>
public sealed record AdditionalCoverageImportSummaryResponse(
    int ModalitiesProcessed,
    int ModalitiesSucceeded,
    int ModalitiesFailed,
    IReadOnlyList<string> Failures);
