using System.Text.Json.Serialization;

namespace SmartInsure.Integration.Casdoor.Models;

/// <summary>
/// Resposta do endpoint de token do Casdoor (grant password). Em falha de credencial o
/// Casdoor responde 200 com <see cref="Error"/> preenchido no lugar do token.
/// </summary>
public sealed record CasdoorTokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; init; }

    [JsonPropertyName("error")]
    public string? Error { get; init; }

    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; init; }
}
