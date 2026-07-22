namespace SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.IgnoreImportedModality.Responses;

/// <summary>Dados de saída do ignorar (RN-034).</summary>
public sealed record IgnoreImportedModalityResponse(
    Guid ImportedModalityId,
    bool Ignored);
