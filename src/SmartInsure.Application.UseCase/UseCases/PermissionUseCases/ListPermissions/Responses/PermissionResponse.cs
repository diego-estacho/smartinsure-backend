namespace SmartInsure.Application.UseCase.UseCases.PermissionUseCases.ListPermissions.Responses;

/// <summary>
/// RN-063: item do catálogo fixo de Permissões da plataforma. <see cref="Area"/> agrupa por domínio
/// (chave estável) e <see cref="DependsOn"/> é o Code da leitura de que uma escrita depende (null na leitura).
/// </summary>
public sealed record PermissionResponse(
    Guid Id,
    string Code,
    string? Description,
    bool IsSystem,
    string? Area,
    string? DependsOn);
