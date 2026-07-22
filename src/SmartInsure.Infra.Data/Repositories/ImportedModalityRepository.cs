using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Infra.Data.Repositories;

public sealed class ImportedModalityRepository(SmartInsureDbContext context)
    : Repository<ImportedModality>(context), IImportedModalityRepository
{
    public async Task<ImportedModality?> GetByInsurerAndSourceAsync(
        Guid insurerId, string sourceId, CancellationToken cancellationToken)
        => await Set.FirstOrDefaultAsync(
            modality => modality.InsurerId == insurerId && modality.SourceId == sourceId,
            cancellationToken);

    public async Task<IReadOnlyList<ImportedModality>> ListActiveByInsurerAsync(
        Guid insurerId, CancellationToken cancellationToken)
        => await Set
            .Where(modality => modality.InsurerId == insurerId
                && modality.Status == EImportedModalityStatus.Active)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ModalityInsurerLinkDto>> ListActiveLinksAsync(CancellationToken cancellationToken)
        => await (
            from imported in Set.AsNoTracking()
            where imported.Status == EImportedModalityStatus.Active
                && !imported.IsIgnored
                && imported.ModalityId != null
            join insurer in Context.Set<Insurer>().AsNoTracking()
                on imported.InsurerId equals insurer.Id
            select new ModalityInsurerLinkDto(
                imported.ModalityId!.Value,
                insurer.Id,
                insurer.CorporateName,
                imported.OriginName,
                imported.Branch.ToString()))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<PendingImportedModalityDto>> ListPendingAsync(CancellationToken cancellationToken)
        => await (
            from imported in Set.AsNoTracking()
            where imported.Status == EImportedModalityStatus.Active
                && !imported.IsIgnored
                && imported.ModalityId == null
            join insurer in Context.Set<Insurer>().AsNoTracking()
                on imported.InsurerId equals insurer.Id
            select new PendingImportedModalityDto(
                imported.Id,
                insurer.Id,
                insurer.CorporateName,
                imported.OriginName,
                imported.Branch.ToString(),
                imported.EngineModalityName,
                Context.Set<ImportedGroup>()
                    .Where(ig => ig.Id == imported.ImportedGroupId)
                    .Select(ig => ig.Name)
                    .FirstOrDefault() ?? string.Empty))
            .ToListAsync(cancellationToken);
}
