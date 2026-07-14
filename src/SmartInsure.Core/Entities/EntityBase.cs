namespace SmartInsure.Core.Entities;

/// <summary>
/// Base obrigatória de toda entidade (ADR-029, ADR-030): identidade UUIDv7 gerada pela
/// aplicação e campos de auditoria preenchidos exclusivamente pelo SaveChangesInterceptor.
/// </summary>
public abstract class EntityBase
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();

    public DateTime CreatedAt { get; private set; }

    public string CreatedBy { get; private set; } = string.Empty;

    public DateTime? UpdatedAt { get; private set; }

    public string? UpdatedBy { get; private set; }
}
