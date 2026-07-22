using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.IgnoreImportedModality.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.IgnoreImportedModality.Requests;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.IgnoreImportedModality.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.IgnoreImportedModality;

/// <summary>RN-034 — marca a Modalidade Importada como Ignorada: sai da Fila e da operação; não volta à fila.</summary>
public sealed class IgnoreImportedModalityUseCase(
    IImportedModalityRepository importedModalityRepository,
    IUnitOfWork unitOfWork) : IIgnoreImportedModalityUseCase
{
    public async Task<IgnoreImportedModalityResponse> ExecuteAsync(
        IgnoreImportedModalityRequest request, CancellationToken cancellationToken)
    {
        var imported = await importedModalityRepository.GetByIdAsync(request.ImportedModalityId, cancellationToken)
            ?? throw new NotFoundException("Modalidade importada não encontrada.");

        imported.Ignore();
        await unitOfWork.CommitAsync(cancellationToken);

        return new IgnoreImportedModalityResponse(imported.Id, imported.IsIgnored);
    }
}
