namespace SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.RestoreImportedModality.Responses;

/// <summary>Dados de saída do reativar (RN-034).</summary>
public sealed record RestoreImportedModalityResponse(
    Guid ImportedModalityId,
    bool Ignored);
