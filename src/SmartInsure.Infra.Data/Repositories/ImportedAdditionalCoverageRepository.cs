using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Infra.Data.Repositories;

public sealed class ImportedAdditionalCoverageRepository(SmartInsureDbContext context)
    : Repository<ImportedAdditionalCoverage>(context), IImportedAdditionalCoverageRepository
{
    public async Task<ImportedAdditionalCoverage?> GetByImportedModalityAndNameAsync(
        Guid importedModalityId, string name, CancellationToken cancellationToken)
        => await Set.FirstOrDefaultAsync(
            coverage => coverage.ImportedModalityId == importedModalityId && coverage.Name == name,
            cancellationToken);

    public async Task<IReadOnlyList<ImportedAdditionalCoverage>> ListActiveByImportedModalityAsync(
        Guid importedModalityId, CancellationToken cancellationToken)
        => await Set
            .Where(coverage => coverage.ImportedModalityId == importedModalityId
                && coverage.Status == EImportedAdditionalCoverageStatus.Active)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<PendingImportedCoverageDto>> ListPendingAsync(CancellationToken cancellationToken)
        => await (
            from coverage in Set.AsNoTracking()
            where coverage.Status == EImportedAdditionalCoverageStatus.Active
                && !coverage.IsIgnored
                && coverage.AdditionalCoverageId == null
            join modality in Context.Set<ImportedModality>().AsNoTracking()
                on coverage.ImportedModalityId equals modality.Id
            join insurer in Context.Set<Insurer>().AsNoTracking()
                on modality.InsurerId equals insurer.Id
            select new PendingImportedCoverageDto(
                coverage.Id, coverage.ImportedModalityId, insurer.CorporateName, modality.OriginName, coverage.Name))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<LinkedImportedCoverageDto>> ListLinkedAsync(CancellationToken cancellationToken)
        => await (
            from coverage in Set.AsNoTracking()
            where coverage.Status == EImportedAdditionalCoverageStatus.Active
                && !coverage.IsIgnored
                && coverage.AdditionalCoverageId != null
            join modality in Context.Set<ImportedModality>().AsNoTracking()
                on coverage.ImportedModalityId equals modality.Id
            join insurer in Context.Set<Insurer>().AsNoTracking()
                on modality.InsurerId equals insurer.Id
            select new LinkedImportedCoverageDto(
                coverage.AdditionalCoverageId!.Value, coverage.Id, insurer.CorporateName, modality.OriginName, coverage.Name))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<OfferableImportedCoverageDto>> ListForQuotationAsync(
        Guid insurerId,
        Guid modalityId,
        IReadOnlyCollection<Guid> additionalCoverageIds,
        CancellationToken cancellationToken)
    {
        // RN-105: Grupo sem cobertura escolhida não precisa de consulta.
        if (additionalCoverageIds.Count == 0)
        {
            return [];
        }

        return await (
            from coverage in Set.AsNoTracking()
            where coverage.Status == EImportedAdditionalCoverageStatus.Active
                && !coverage.IsIgnored
                && coverage.AdditionalCoverageId != null
                && additionalCoverageIds.Contains(coverage.AdditionalCoverageId.Value)
            join modality in Context.Set<ImportedModality>().AsNoTracking()
                on coverage.ImportedModalityId equals modality.Id
            where modality.InsurerId == insurerId
                && modality.ModalityId == modalityId
                && modality.Status == EImportedModalityStatus.Active
                && !modality.IsIgnored
            // RN-046: canônica Inativa não é oferecida nem enviada.
            join canonical in Context.Set<AdditionalCoverage>().AsNoTracking()
                on coverage.AdditionalCoverageId equals canonical.Id
            where canonical.Status == EAdditionalCoverageStatus.Active
            select new OfferableImportedCoverageDto(canonical.Id, coverage.Id, coverage.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AvailableAdditionalCoverageDto>> ListAvailableForModalityAsync(
        Guid brokerageId,
        Guid modalityId,
        CancellationToken cancellationToken)
        => await (
            from coverage in Set.AsNoTracking()
            where coverage.Status == EImportedAdditionalCoverageStatus.Active
                && !coverage.IsIgnored
                && coverage.AdditionalCoverageId != null
            join modality in Context.Set<ImportedModality>().AsNoTracking()
                on coverage.ImportedModalityId equals modality.Id
            where modality.ModalityId == modalityId
                && modality.Status == EImportedModalityStatus.Active
                && !modality.IsIgnored
            // RN-104/RN-103: só Seguradoras habilitadas (Ativas) da Corretora do Escopo ativo.
            join enablement in Context.Set<BrokerageInsurerEnablement>().AsNoTracking()
                on modality.InsurerId equals enablement.InsurerId
            where enablement.BrokerageId == brokerageId
                && enablement.Status == EBrokerageInsurerEnablementStatus.Active
            join canonical in Context.Set<AdditionalCoverage>().AsNoTracking()
                on coverage.AdditionalCoverageId equals canonical.Id
            where canonical.Status == EAdditionalCoverageStatus.Active
            select new AvailableAdditionalCoverageDto(canonical.Id, canonical.Name))
            .Distinct()
            .ToListAsync(cancellationToken);
}
