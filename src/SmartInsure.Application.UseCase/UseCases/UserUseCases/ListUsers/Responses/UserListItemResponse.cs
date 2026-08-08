namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.ListUsers.Responses;

/// <summary>
/// Item da listagem de Usuários. A situação (`Status`) sai pelo nome estável do enum (ADR-031);
/// `InviteExpired` é o flag que, com a situação Pendente, a tela mostra como "Convite expirado"
/// (RN-065). `Link` é o Vínculo (Corretora/Tomador); nulo no Escopo Sistema.
/// </summary>
public sealed record UserListItemResponse(
    Guid Id,
    string Name,
    string Email,
    string Status,
    string? ProfileName,
    string? ProfileScope,
    bool ProfileIsFixed,
    string? Link,
    DateTime CreatedAt,
    bool InviteExpired,
    DateTime? LastAccessAtUtc);
