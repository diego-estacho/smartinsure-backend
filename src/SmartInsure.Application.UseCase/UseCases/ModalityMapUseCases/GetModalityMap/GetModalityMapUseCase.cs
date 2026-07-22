using SmartInsure.Application.UseCase.UseCases.ModalityMapUseCases.GetModalityMap.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ModalityMapUseCases.GetModalityMap.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityMapUseCases.GetModalityMap.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;

namespace SmartInsure.Application.UseCase.UseCases.ModalityMapUseCases.GetModalityMap;

/// <summary>
/// RN-033/RN-034 — monta o Mapa: cada Modalidade Ativa com as Seguradoras que a oferecem
/// (mapeamento Confirmado ativo), a disponibilidade por ramo derivada, e a Fila de pendências.
/// Oferecida = tem ao menos uma Modalidade Importada Ativa Confirmada.
/// </summary>
public sealed class GetModalityMapUseCase(
    IModalityRepository modalityRepository,
    IModalityMappingRepository modalityMappingRepository,
    IImportedModalityRepository importedModalityRepository) : IGetModalityMapUseCase
{
    public async Task<ModalityMapResponse> ExecuteAsync(
        GetModalityMapRequest request, CancellationToken cancellationToken)
    {
        var modalities = await modalityRepository.ListActiveForMapAsync(cancellationToken);
        var confirmed = await modalityMappingRepository.ListConfirmedActiveAsync(cancellationToken);
        var pending = await importedModalityRepository.ListPendingAsync(cancellationToken);

        var confirmedByModality = confirmed
            .GroupBy(mapping => mapping.ModalityId)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<ConfirmedMappingDto>)group.ToList());

        var entries = modalities
            .Select(modality =>
            {
                var insurers = confirmedByModality.TryGetValue(modality.Id, out var list)
                    ? list
                    : [];

                return new ModalityMapEntryResponse(
                    modality.Id,
                    modality.Name,
                    modality.ModalityGroupName,
                    modality.Status,
                    insurers.Count > 0,
                    insurers.Select(insurer => insurer.Branch).Distinct().OrderBy(branch => branch).ToList(),
                    insurers
                        .Select(insurer => new MapInsurerResponse(
                            insurer.InsurerId, insurer.InsurerName, insurer.ImportedModalityId, insurer.OriginName))
                        .ToList());
            })
            .ToList();

        var pendingResponse = pending
            .Select(item => new PendingImportedModalityResponse(
                item.ImportedModalityId, item.InsurerId, item.InsurerName, item.OriginName,
                item.Branch, item.EngineModalityName, item.GroupName))
            .ToList();

        return new ModalityMapResponse(entries, pendingResponse);
    }
}
