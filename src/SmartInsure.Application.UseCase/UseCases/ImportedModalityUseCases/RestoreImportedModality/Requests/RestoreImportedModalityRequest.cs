namespace SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.RestoreImportedModality.Requests;

/// <summary>Reativar (desfazer Ignorar) uma Modalidade Importada (RN-037). O id da Importada vem da rota.</summary>
public sealed record RestoreImportedModalityRequest(Guid ImportedModalityId);
