using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.RestoreImportedModality.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.RestoreImportedModality.Requests;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.RestoreImportedModality.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.RestoreImportedModality;

/// <summary>RN-037 — reativar: desfaz o Ignorar de uma Modalidade Importada, que volta a poder ser oferecida.</summary>
public sealed class RestoreImportedModalityUseCase(
    IImportedModalityRepository importedModalityRepository,
    IUnitOfWork unitOfWork) : IRestoreImportedModalityUseCase
{
    public async Task<RestoreImportedModalityResponse> ExecuteAsync(
        RestoreImportedModalityRequest request, CancellationToken cancellationToken)
    {
        var imported = await importedModalityRepository.GetByIdAsync(request.ImportedModalityId, cancellationToken)
            ?? throw new NotFoundException("Modalidade importada não encontrada.");

        imported.Restore();
        await unitOfWork.CommitAsync(cancellationToken);

        return new RestoreImportedModalityResponse(imported.Id, imported.IsIgnored);
    }
}
