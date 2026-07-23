namespace SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.ReassignImportedModality.Responses;

/// <summary>Dados de saída da reatribuição manual (RN-037): o vínculo passa a Manual.</summary>
public sealed record ReassignImportedModalityResponse(
    Guid ImportedModalityId,
    Guid ModalityId,
    string LinkSource);
