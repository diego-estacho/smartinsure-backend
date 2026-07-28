using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Infra.Data.Repositories;

/// <summary>Repositório do Grupo de Cotação (RN-050/RN-051).</summary>
public sealed class QuotationGroupRepository(SmartInsureDbContext context)
    : Repository<QuotationGroup>(context), IQuotationGroupRepository
{
    // Rastreado (sem AsNoTracking): o UseCase de atualização muta a raiz e recria a coleção do escopo,
    // e o change tracker resolve inserts/deletes dos filhos antes do commit do UnitOfWork.
    public async Task<QuotationGroup?> GetByIdWithInsurersAsync(Guid id, CancellationToken cancellationToken)
        => await Set
            .Include(group => group.SelectedInsurers)
            .FirstOrDefaultAsync(group => group.Id == id, cancellationToken);

    public async Task<QuotationContextDto?> GetContextAsync(
        Guid groupId, Guid brokerageId, CancellationToken cancellationToken)
        => await (
            from quotationGroup in context.QuotationGroups.AsNoTracking()
            where quotationGroup.Id == groupId
            join policyHolder in context.Persons on quotationGroup.PolicyHolderId equals policyHolder.Id
            join insured in context.Persons on quotationGroup.InsuredId equals insured.Id
            join modality in context.Modalities on quotationGroup.ModalityId equals modality.Id
            join broker in context.Persons on brokerageId equals broker.Id
            select new QuotationContextDto(
                broker.DocumentNumber,
                policyHolder.DocumentNumber,
                insured.DocumentNumber,
                modality.GlobalModalityExternalId,
                modality.Name,
                quotationGroup.InsuredAmount,
                quotationGroup.CoverageStartDate,
                quotationGroup.CoverageEndDate,
                quotationGroup.IncludesPenaltyCoverage,
                quotationGroup.IncludesLaborCoverage))
            .FirstOrDefaultAsync(cancellationToken);
}
