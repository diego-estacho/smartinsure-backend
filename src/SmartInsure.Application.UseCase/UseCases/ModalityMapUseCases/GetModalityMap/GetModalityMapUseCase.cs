using SmartInsure.Application.UseCase.UseCases.ModalityMapUseCases.GetModalityMap.Interfaces;
using SmartInsure.Application.UseCase.UseCases.ModalityMapUseCases.GetModalityMap.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityMapUseCases.GetModalityMap.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;

namespace SmartInsure.Application.UseCase.UseCases.ModalityMapUseCases.GetModalityMap;

/// <summary>
/// RN-036/RN-037 — monta o Mapa: cada Modalidade Ativa com as Seguradoras que a oferecem
/// (uma entrada por Seguradora distinta, com contagem — ADR-061), a disponibilidade por ramo
/// derivada, e a Fila de exceções. Oferecida = tem ao menos uma Modalidade Importada Ativa,
/// não Ignorada, vinculada.
/// </summary>
public sealed class GetModalityMapUseCase(
    IModalityRepository modalityRepository,
    IImportedModalityRepository importedModalityRepository) : IGetModalityMapUseCase
{
    public async Task<ModalityMapResponse> ExecuteAsync(
        GetModalityMapRequest request, CancellationToken cancellationToken)
    {
        var modalities = await modalityRepository.ListActiveForMapAsync(cancellationToken);
        var links = await importedModalityRepository.ListActiveLinksAsync(cancellationToken);
        var pending = await importedModalityRepository.ListPendingAsync(cancellationToken);

        var linksByModality = links
            .GroupBy(link => link.ModalityId)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<ModalityInsurerLinkDto>)group.ToList());

        var entries = modalities
            .Select(modality =>
            {
                var modalityLinks = linksByModality.TryGetValue(modality.Id, out var list) ? list : [];

                // RN-036: uma entrada por Seguradora distinta, preservando a ordem de primeira ocorrência.
                var insurers = modalityLinks
                    .GroupBy(link => link.InsurerId)
                    .Select(group => new MapInsurerResponse(
                        group.Key,
                        group.First().InsurerName,
                        group.Count(),
                        group.Select(link => link.OriginName).ToList()))
                    .ToList();

                return new ModalityMapEntryResponse(
                    modality.Id,
                    modality.Name,
                    modality.Status,
                    insurers.Count > 0,
                    modalityLinks.Select(link => link.Branch).Distinct().OrderBy(branch => branch).ToList(),
                    insurers);
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
