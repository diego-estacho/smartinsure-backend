using SmartInsure.Core.Exceptions;

namespace SmartInsure.Core.Entities;

/// <summary>
/// Permissão (RN-063): autorização atômica de uma operação, identificada por um Code único.
/// Catálogo declarado pela plataforma; <see cref="IsSystem"/> marca a Permissão fixa em código.
/// </summary>
public sealed class Permission : EntityBase
{
    private Permission()
    {
    }

    public string Code { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public bool IsSystem { get; private set; }

    /// <summary>
    /// RN-063 (revisão 2026-08-07): Área do catálogo à qual a Permissão pertence (chave estável em
    /// inglês, ex.: <c>quotations</c>, <c>policy-holders</c>). Agrupa a Permissão para exibição e
    /// para os níveis Sem acesso · Consultar · Operar. Semeada por migration.
    /// </summary>
    public string? Area { get; private set; }

    /// <summary>
    /// RN-063 (revisão 2026-08-07): quando a Permissão é uma ação de escrita, o <see cref="Code"/>
    /// da Permissão de leitura de que ela depende — marcar a ação liga a leitura; desmarcar a
    /// leitura derruba em cascata. Leitura tem <c>null</c>. Semeado por migration.
    /// </summary>
    public string? DependsOn { get; private set; }

    public static Permission Create(
        string code,
        string? description,
        bool isSystem,
        string? area = null,
        string? dependsOn = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new BusinessRuleException("A permissão precisa de um código.");
        }

        return new Permission
        {
            Code = code.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            IsSystem = isSystem,
            Area = string.IsNullOrWhiteSpace(area) ? null : area.Trim(),
            DependsOn = string.IsNullOrWhiteSpace(dependsOn) ? null : dependsOn.Trim(),
        };
    }
}
