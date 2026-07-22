using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.MapImportedModality.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.MapImportedModality.Requests;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.MapImportedModality.Responses;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.MapImportedModality;

/// <summary>
/// RN-034 — mapeia uma Modalidade Importada pendente para uma Modalidade existente: nasce
/// Confirmado manual (com quem/quando). Trava de ramo (RN-032): não mapeia para Modalidade que
/// já tem Importada Ativa Confirmada de ramo diferente.
/// </summary>
public sealed class MapImportedModalityUseCase(
    IImportedModalityRepository importedModalityRepository,
    IModalityRepository modalityRepository,
    IModalityMappingRepository modalityMappingRepository,
    ICurrentUserAccessor currentUserAccessor,
    IUnitOfWork unitOfWork) : IMapImportedModalityUseCase
{
    public async Task<MapImportedModalityResponse> ExecuteAsync(
        MapImportedModalityRequest request, CancellationToken cancellationToken)
    {
        var imported = await importedModalityRepository.GetByIdAsync(request.ImportedModalityId, cancellationToken)
            ?? throw new NotFoundException("Modalidade importada não encontrada.");

        _ = await modalityRepository.GetByIdAsync(request.ModalityId, cancellationToken)
            ?? throw new NotFoundException("Modalidade não encontrada no catálogo.");

        if (await modalityMappingRepository.HasConfirmedAsync(imported.Id, cancellationToken))
        {
            throw new ConflictException("A modalidade importada já possui um mapeamento confirmado.");
        }

        if (await importedModalityRepository.HasConfirmedBranchConflictAsync(
            request.ModalityId, imported.Branch, cancellationToken))
        {
            throw new BusinessRuleException(
                "A modalidade de destino é de ramo incompatível com esta modalidade importada.");
        }

        var mapping = ModalityMapping.CreateManual(
            imported.Id, request.ModalityId, currentUserAccessor.UserIdentifier ?? "sistema", DateTime.UtcNow);

        await modalityMappingRepository.AddAsync(mapping, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return new MapImportedModalityResponse(imported.Id, request.ModalityId, mapping.Status.ToString());
    }
}
