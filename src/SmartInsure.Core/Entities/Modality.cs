using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Core.Entities;

/// <summary>
/// Modalidade (RN-029/RN-036): o item do catálogo que o corretor vê e o eixo de comparação entre
/// Seguradoras. Derivada da Modalidade Global da OnPoint na importação — identidade pelo id global
/// (<see cref="GlobalModalityExternalId"/>), nome da fonte — ou criada manualmente pelo Administrador
/// (ADR-061). Não há Grupo no lado Smart; nunca é excluída (RN-036).
/// </summary>
public sealed class Modality : EntityBase
{
    private Modality()
    {
    }

    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Id da Modalidade Global da OnPoint (RN-032). Presente nas Modalidades derivadas da importação
    /// (único quando presente); nulo nas criadas manualmente.
    /// </summary>
    public string? GlobalModalityExternalId { get; private set; }

    public string? Description { get; private set; }

    public EModalityStatus Status { get; private set; }

    /// <summary>RN-029: Modalidade criada manualmente pelo Administrador do Sistema (sem id global).</summary>
    public static Modality CreateManual(string name, string? description, EModalityStatus initialStatus)
    {
        var modality = new Modality { Status = initialStatus };
        modality.SetDetails(name, description);
        return modality;
    }

    /// <summary>
    /// RN-029/RN-032: Modalidade derivada da Modalidade Global na importação. Nasce Ativa, com a
    /// identidade (id global) e o nome vindos da fonte.
    /// </summary>
    public static Modality CreateFromGlobal(string globalModalityExternalId, string name)
    {
        var modality = new Modality
        {
            Status = EModalityStatus.Active,
            GlobalModalityExternalId = globalModalityExternalId.Trim(),
        };
        modality.SetDetails(name, description: null);
        return modality;
    }

    /// <summary>RN-029: edição de curadoria (nome/descrição); a situação e o id global não mudam aqui.</summary>
    public void Update(string name, string? description)
        => SetDetails(name, description);

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

    private void SetDetails(string name, string? description)
    {
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }
}
