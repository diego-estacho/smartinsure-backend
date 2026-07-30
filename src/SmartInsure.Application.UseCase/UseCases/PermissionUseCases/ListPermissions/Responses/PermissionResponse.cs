namespace SmartInsure.Application.UseCase.UseCases.PermissionUseCases.ListPermissions.Responses;

/// <summary>RN-063: item do catálogo fixo de Permissões da plataforma.</summary>
public sealed record PermissionResponse(
    Guid Id,
    string Code,
    string? Description,
    bool IsSystem);
