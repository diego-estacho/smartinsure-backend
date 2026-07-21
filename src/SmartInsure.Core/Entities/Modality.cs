using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Core.Entities;

/// <summary>
/// Modalidade (RN-029/RN-036): o item do catálogo que o corretor vê e o eixo de comparação entre Seguradoras.
/// Curada pela equipe SmartInsure — nunca criada pela importação (ADR-060); nunca excluída (RN-036).
/// Não carrega Ramo: a trava de ramo vive no lado importado (ADR-060).
/// </summary>
public sealed class Modality : EntityBase
{
    private Modality()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public Guid ModalityGroupId { get; private set; }

    public string? Description { get; private set; }

    public EModalityStatus Status { get; private set; }

    public static Modality Create(
        string name, Guid modalityGroupId, string? description, EModalityStatus initialStatus)
    {
        var modality = new Modality { Status = initialStatus };
        modality.SetDetails(name, modalityGroupId, description);
        return modality;
    }

    /// <summary>RN-029: edição de curadoria; a situação não muda aqui.</summary>
    public void Update(string name, Guid modalityGroupId, string? description)
        => SetDetails(name, modalityGroupId, description);

    /// <summary>RN-036: Inativa → Ativa; ativar quem já está Ativa é conflito de estado.</summary>
    public void Activate()
    {
        if (Status == EModalityStatus.Active)
        {
            throw new ConflictException("A modalidade já está ativa.");
        }

        Status = EModalityStatus.Active;
    }

    /// <summary>RN-036: Ativa → Inativa (fora de operação, nunca excluída).</summary>
    public void Deactivate()
    {
        if (Status == EModalityStatus.Inactive)
        {
            throw new ConflictException("A modalidade já está inativa.");
        }

        Status = EModalityStatus.Inactive;
    }

    private void SetDetails(string name, Guid modalityGroupId, string? description)
    {
        Name = name.Trim();
        ModalityGroupId = modalityGroupId;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }
}
