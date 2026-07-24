using SmartInsure.Core.Enumerators;

namespace SmartInsure.Core.Entities;

/// <summary>
/// Cobertura Adicional (RN-040): o item canônico do Smart — garantia complementar nomeada de uma
/// Modalidade, sem dono de Seguradora, que o corretor vê na cotação. Curada pelo Administrador do
/// Sistema (criar/editar/ativar/inativar), nome único no catálogo. Nunca criada pela importação;
/// as versões de cada Seguradora são as Coberturas Adicionais Importadas vinculadas a ela (RN-043).
/// Nunca excluída; sai de operação por Inativação (RN-044/RN-046).
/// </summary>
public sealed class AdditionalCoverage : EntityBase
{
    private AdditionalCoverage()
    {
    }

    /// <summary>Nome canônico normalizado (trim) — único no catálogo (RN-040).</summary>
    public string Name { get; private set; } = string.Empty;

    public EAdditionalCoverageStatus Status { get; private set; }

    public static AdditionalCoverage Create(string name)
        => new()
        {
            Name = name.Trim(),
            Status = EAdditionalCoverageStatus.Active,
        };

    /// <summary>RN-040: o Administrador edita o nome canônico.</summary>
    public void Rename(string name) => Name = name.Trim();

    /// <summary>RN-040/RN-046: o Administrador ativa a Cobertura Adicional canônica.</summary>
    public void Activate() => Status = EAdditionalCoverageStatus.Active;

    /// <summary>RN-040/RN-046: o Administrador inativa a Cobertura Adicional canônica (não é excluída).</summary>
    public void Deactivate() => Status = EAdditionalCoverageStatus.Inactive;
}
