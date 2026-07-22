namespace SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.MapImportedModality.Requests;

/// <summary>Mapear uma pendência da Fila para uma Modalidade existente (RN-034). O id da Importada vem da rota.</summary>
public sealed record MapImportedModalityRequest(
    Guid ImportedModalityId,
    Guid ModalityId);
