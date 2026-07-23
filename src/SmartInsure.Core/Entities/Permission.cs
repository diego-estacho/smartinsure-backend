using SmartInsure.Core.Exceptions;

namespace SmartInsure.Core.Entities;

/// <summary>
/// Permissão (RN-033): autorização atômica de uma operação, identificada por um Code único.
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

    public static Permission Create(string code, string? description, bool isSystem)
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
        };
    }
}
