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

    /// <summary>
    /// Busca a identidade pelo UUID do Casdoor (o <c>ExternalIdentity</c> guardado pela plataforma).
    /// O parâmetro é <c>userId</c>, não <c>id</c>: para o Casdoor, <c>id</c> é <c>owner/name</c> e um
    /// UUID puro falha com <c>GetOwnerAndNameFromId() error, wrong token count for ID</c>.
    /// </summary>
    [Get("/api/get-user")]
    Task<CasdoorResponse<CasdoorUser?>> GetUserAsync(
        [AliasAs("userId")] string userId,
        CancellationToken cancellationToken);

    [Post("/api/add-user")]
    Task<CasdoorResponse<object>> AddUserAsync(
        [Body] CasdoorUser user,
        CancellationToken cancellationToken);

    [Post("/api/delete-user")]
    Task<CasdoorResponse<object>> DeleteUserAsync(
        [Body] CasdoorUser user,
        CancellationToken cancellationToken);

    /// <summary>
    /// RN-065: define a senha da identidade. Endpoint dedicado do Casdoor — <c>update-user</c> com
    /// <c>password</c> no corpo responde <c>ok</c> mas não grava a credencial (nem com
    /// <c>columns=password</c>), então o grant seguinte falharia. Este endpoint também zera
    /// <c>needUpdatePassword</c>, cumprindo a troca obrigatória do primeiro acesso.
    /// </summary>
    [Post("/api/set-password")]
    Task<CasdoorResponse<object>> SetPasswordAsync(
        [Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> form,
        CancellationToken cancellationToken);

    /// <summary>
    /// RN-005: validação de credenciais via grant password do OAuth do Casdoor.
    /// O token retornado não é repassado ao cliente — a plataforma emite o próprio acesso.
    /// </summary>
    [Post("/api/login/oauth/access_token")]
    Task<CasdoorTokenResponse> RequestTokenAsync(
        [Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> form,
        CancellationToken cancellationToken);
}
