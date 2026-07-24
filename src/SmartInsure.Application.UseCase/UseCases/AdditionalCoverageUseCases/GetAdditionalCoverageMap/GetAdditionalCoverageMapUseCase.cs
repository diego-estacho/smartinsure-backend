using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.GetAdditionalCoverageMap.Interfaces;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.GetAdditionalCoverageMap.Requests;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.GetAdditionalCoverageMap.Responses;
using SmartInsure.Core.Abstractions.Repositories;

namespace SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.GetAdditionalCoverageMap.Requests
{
    /// <summary>Consulta o Mapa da curadoria de Coberturas Adicionais (RN-043/RN-046). Sem parâmetros.</summary>
    public sealed record GetAdditionalCoverageMapRequest;
}

namespace SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.GetAdditionalCoverageMap.Responses
{
    /// <summary>Uma Cobertura Adicional Importada Ativa vinculada a uma canônica.</summary>
    public sealed record LinkedCoverageItem(
        Guid ImportedCoverageId, string InsurerName, string ModalityName, string CoverageName);

    /// <summary>Uma Cobertura Adicional canônica com as importadas Ativas vinculadas a ela.</summary>
    public sealed record CanonicalCoverageItem(
        Guid Id, string Name, string Status, IReadOnlyList<LinkedCoverageItem> Linked);

    /// <summary>Uma Cobertura Adicional Importada pendente de mapeamento (Fila).</summary>
    public sealed record PendingCoverageItem(
        Guid Id, Guid ImportedModalityId, string InsurerName, string ModalityName, string CoverageName);

    /// <summary>Mapa da curadoria: catálogo canônico (com vínculos) e a Fila de pendências.</summary>
    public sealed record AdditionalCoverageMapResponse(
        IReadOnlyList<CanonicalCoverageItem> Coverages,
        IReadOnlyList<PendingCoverageItem> Pending);
}

namespace SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.GetAdditionalCoverageMap.Interfaces
{
    public interface IGetAdditionalCoverageMapUseCase
        : IUseCase<GetAdditionalCoverageMapRequest, AdditionalCoverageMapResponse>;
}

namespace SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.GetAdditionalCoverageMap
{
    /// <summary>
    /// RN-043/RN-046 — monta o Mapa da curadoria: cada Cobertura Adicional canônica com as suas
    /// Coberturas Adicionais Importadas Ativas vinculadas, e a Fila de importadas pendentes de mapeamento.
    /// </summary>
    public sealed class GetAdditionalCoverageMapUseCase(
        IAdditionalCoverageRepository additionalCoverageRepository,
        IImportedAdditionalCoverageRepository importedAdditionalCoverageRepository)
        : IGetAdditionalCoverageMapUseCase
    {
        public async Task<AdditionalCoverageMapResponse> ExecuteAsync(
            GetAdditionalCoverageMapRequest request, CancellationToken cancellationToken)
        {
            var canonical = await additionalCoverageRepository.ListAllAsync(cancellationToken);
            var linked = await importedAdditionalCoverageRepository.ListLinkedAsync(cancellationToken);
            var pending = await importedAdditionalCoverageRepository.ListPendingAsync(cancellationToken);

            var linkedByCoverage = linked
                .GroupBy(link => link.AdditionalCoverageId)
                .ToDictionary(
                    group => group.Key,
                    group => (IReadOnlyList<LinkedCoverageItem>)group
                        .Select(link => new LinkedCoverageItem(
                            link.ImportedCoverageId, link.InsurerName, link.ModalityName, link.CoverageName))
                        .ToList());

            var coverages = canonical
                .Select(coverage => new CanonicalCoverageItem(
                    coverage.Id,
                    coverage.Name,
                    coverage.Status,
                    linkedByCoverage.TryGetValue(coverage.Id, out var items) ? items : []))
                .ToList();

            var pendingItems = pending
                .Select(item => new PendingCoverageItem(
                    item.Id, item.ImportedModalityId, item.InsurerName, item.ModalityName, item.CoverageName))
                .ToList();

            return new AdditionalCoverageMapResponse(coverages, pendingItems);
        }
    }
}
