using Refit;
using SmartInsure.Integration.Casdoor.Models;

namespace SmartInsure.Integration.Casdoor.Interfaces;

/// <summary>API de gestão do Casdoor; autenticação Basic clientId:secret no handler do HttpClient.</summary>
public interface ICasdoorApi
{
    [Get("/api/get-user")]
    Task<CasdoorResponse<CasdoorUser?>> GetUserByEmailAsync(
        [AliasAs("email")] string email,
        CancellationToken cancellationToken);

    [Get("/api/get-user")]
    Task<CasdoorResponse<CasdoorUser?>> GetUserAsync(
        [AliasAs("id")] string id,
        CancellationToken cancellationToken);

    [Post("/api/add-user")]
    Task<CasdoorResponse<object>> AddUserAsync(
        [Body] CasdoorUser user,
        CancellationToken cancellationToken);

    [Post("/api/delete-user")]
    Task<CasdoorResponse<object>> DeleteUserAsync(
        [Body] CasdoorUser user,
        CancellationToken cancellationToken);
}
