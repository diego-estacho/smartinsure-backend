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
                && coverage.AdditionalCoverageId != null
            join modality in Context.Set<ImportedModality>().AsNoTracking()
                on coverage.ImportedModalityId equals modality.Id
            join insurer in Context.Set<Insurer>().AsNoTracking()
                on modality.InsurerId equals insurer.Id
            select new LinkedImportedCoverageDto(
                coverage.AdditionalCoverageId!.Value, coverage.Id, insurer.CorporateName, modality.OriginName, coverage.Name))
            .ToListAsync(cancellationToken);
}
