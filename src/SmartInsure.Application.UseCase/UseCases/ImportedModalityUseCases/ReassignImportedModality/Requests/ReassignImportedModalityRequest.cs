namespace SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.ReassignImportedModality.Requests;

/// <summary>Reatribuir manualmente uma Modalidade Importada a uma Modalidade (RN-034). O id da Importada vem da rota.</summary>
public sealed record ReassignImportedModalityRequest(
    Guid ImportedModalityId,
    Guid ModalityId);
