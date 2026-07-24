using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
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
}
