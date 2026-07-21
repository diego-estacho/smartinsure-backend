using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Core.Entities;

/// <summary>
/// Grupo de Modalidade (RN-029/RN-036): agrupador curado pela equipe SmartInsure, mantido somente
/// pelo Administrador do Sistema; nunca excluído — Inativa esconde o grupo e suas Modalidades da operação (RN-036).
/// </summary>
public sealed class ModalityGroup : EntityBase
{
    private ModalityGroup()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public int DisplayOrder { get; private set; }

    public EModalityGroupStatus Status { get; private set; }

    public static ModalityGroup Create(
        string name, string? description, int displayOrder, EModalityGroupStatus initialStatus)
    {
        var group = new ModalityGroup { Status = initialStatus };
        group.SetDetails(name, description, displayOrder);
        return group;
    }

    /// <summary>RN-029: edição de curadoria; a situação não muda aqui.</summary>
    public void Update(string name, string? description, int displayOrder)
        => SetDetails(name, description, displayOrder);

    /// <summary>RN-036: Inativo → Ativo; ativar quem já está Ativo é conflito de estado.</summary>
    public void Activate()
    {
        if (Status == EModalityGroupStatus.Active)
        {
            throw new ConflictException("O grupo de modalidade já está ativo.");
        }

        Status = EModalityGroupStatus.Active;
    }

    /// <summary>RN-036: Ativo → Inativo (fora de operação, nunca excluído).</summary>
    public void Deactivate()
    {
        if (Status == EModalityGroupStatus.Inactive)
        {
            throw new ConflictException("O grupo de modalidade já está inativo.");
        }

        Status = EModalityGroupStatus.Inactive;
    }

    private void SetDetails(string name, string? description, int displayOrder)
    {
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        DisplayOrder = displayOrder;
    }
}
