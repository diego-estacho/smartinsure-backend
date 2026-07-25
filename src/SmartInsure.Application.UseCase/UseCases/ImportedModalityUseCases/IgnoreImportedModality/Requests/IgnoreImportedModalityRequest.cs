namespace SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.IgnoreImportedModality.Requests;

/// <summary>Ignorar uma pendência da Fila (RN-037). O id da Importada vem da rota.</summary>
public sealed record IgnoreImportedModalityRequest(Guid ImportedModalityId);
