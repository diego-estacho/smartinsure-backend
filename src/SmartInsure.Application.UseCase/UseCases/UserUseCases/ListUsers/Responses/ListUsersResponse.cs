namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.ListUsers.Responses;

/// <summary>
/// Resposta da listagem de Usuários: a página + as contagens por situação para as abas da tela.
/// As contagens respeitam Escopo (RN-064) e busca, mas não o próprio filtro de situação.
/// </summary>
public sealed record ListUsersResponse(
    IReadOnlyList<UserListItemResponse> Items,
    int Page,
    int PageSize,
    long TotalCount,
    UserStatusCountsResponse Counts);

/// <summary>Contagens por situação: "Pendente" é o Pendente não expirado; "Expirado" é o Pendente com Convite vencido (RN-065).</summary>
public sealed record UserStatusCountsResponse(
    long All,
    long Active,
    long Pending,
    long Expired,
    long Inactive);
