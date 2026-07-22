namespace SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.MapImportedModality.Responses;

/// <summary>Dados de saída do mapeamento manual (RN-034).</summary>
public sealed record MapImportedModalityResponse(
    Guid ImportedModalityId,
    Guid ModalityId,
    string MappingStatus);
