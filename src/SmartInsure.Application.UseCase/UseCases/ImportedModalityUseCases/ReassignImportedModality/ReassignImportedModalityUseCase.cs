using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.ReassignImportedModality.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.ReassignImportedModality.Requests;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.ReassignImportedModality.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.ReassignImportedModality;

/// <summary>
/// RN-034 — reatribui manualmente uma Modalidade Importada a uma Modalidade: o vínculo passa a
/// Manual e é preservado na reimportação (RN-032). A Modalidade de destino precisa existir.
/// </summary>
public sealed class ReassignImportedModalityUseCase(
    IImportedModalityRepository importedModalityRepository,
    IModalityRepository modalityRepository,
    IUnitOfWork unitOfWork) : IReassignImportedModalityUseCase
{
    public async Task<ReassignImportedModalityResponse> ExecuteAsync(
        ReassignImportedModalityRequest request, CancellationToken cancellationToken)
    {
        var imported = await importedModalityRepository.GetByIdAsync(request.ImportedModalityId, cancellationToken)
            ?? throw new NotFoundException("Modalidade importada não encontrada.");

        _ = await modalityRepository.GetByIdAsync(request.ModalityId, cancellationToken)
            ?? throw new NotFoundException("Modalidade não encontrada no catálogo.");

        imported.LinkToModality(request.ModalityId, EModalityLinkSource.Manual);
        await unitOfWork.CommitAsync(cancellationToken);

        return new ReassignImportedModalityResponse(
            imported.Id, request.ModalityId, imported.ModalityLinkSource!.Value.ToString());
    }
}
