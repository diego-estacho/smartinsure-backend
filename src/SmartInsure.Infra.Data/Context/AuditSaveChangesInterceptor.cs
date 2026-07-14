using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;

namespace SmartInsure.Infra.Data.Context;

/// <summary>
/// Preenche a auditoria de toda entidade (ADR-030) — código de aplicação nunca atribui
/// esses campos. Accessor nulo significa execução de sistema (ADR-035), auditada como "sistema".
/// </summary>
public sealed class AuditSaveChangesInterceptor(ICurrentUserAccessor? currentUserAccessor)
    : SaveChangesInterceptor
{
    private const string SystemUser = "sistema";

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAudit(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var user = currentUserAccessor?.UserIdentifier ?? SystemUser;
        var now = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<EntityBase>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(EntityBase.CreatedAt)).CurrentValue = now;
                entry.Property(nameof(EntityBase.CreatedBy)).CurrentValue = user;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(EntityBase.UpdatedAt)).CurrentValue = now;
                entry.Property(nameof(EntityBase.UpdatedBy)).CurrentValue = user;
            }
        }
    }
}
