using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Integration.Casdoor.Interfaces;
using SmartInsure.Integration.Casdoor.Models;
using SmartInsure.Integration.Casdoor.Options;

namespace SmartInsure.Integration.Casdoor.Services;

/// <summary>
/// Implementação do provedor de identidade sobre a API de gestão do Casdoor.
/// A identidade nasce com a senha inicial padrão e NeedUpdatePassword, cumprindo a
/// troca obrigatória no primeiro acesso (RN-001).
/// </summary>
public sealed partial class CasdoorIdentityProvider(
    ICasdoorApi api,
    IOptions<CasdoorOptions> options) : IIdentityProvider
{
    private const int MaxUsernameLength = 39;

    private readonly CasdoorOptions _options = options.Value;

    [GeneratedRegex("[^a-zA-Z0-9]")]
    private static partial Regex NonAlphanumericRegex();

    /// <summary>
    /// Username aceito pelo Casdoor (só alfanumérico/underline): prefixo de ambiente +
    /// e-mail com não-alfanuméricos como underline, limitado a 39 chars, sem underline
    /// final, em minúsculas. Mesma derivação usada no InsurePoint legado.
    /// </summary>
    internal static string GetUsername(string environmentPrefix, string email)
    {
        var username = $"{environmentPrefix}_{NonAlphanumericRegex().Replace(email, "_")}";
        username = username.Length > MaxUsernameLength ? username[..MaxUsernameLength] : username;

        if (username.EndsWith('_'))
        {
            username = $"{username[..^1]}0";
        }

        return username.ToLowerInvariant();
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
    {
        var response = await api.GetUserByEmailAsync(email, cancellationToken);

        return response.IsOk && response.Data is not null;
    }

    public async Task<string> CreateIdentityAsync(string name, string email, CancellationToken cancellationToken)
    {
        var user = new CasdoorUser
        {
            Owner = _options.OrganizationName,
            Name = GetUsername(_options.EnviromentUserCasdoor, email),
            DisplayName = name,
            Email = email,
            Password = _options.DefaultPassword,
            SignupApplication = _options.AppName,
            NeedUpdatePassword = true,
        };

        var response = await api.AddUserAsync(user, cancellationToken);

        if (!response.IsOk)
        {
            throw new InvalidOperationException(
                $"Provedor de identidade recusou a criação da identidade: {response.Msg}");
        }

        var created = await api.GetUserByEmailAsync(email, cancellationToken);

        return created.Data?.Id
            ?? throw new InvalidOperationException(
                "Provedor de identidade não retornou o identificador da identidade criada.");
    }

    public async Task RemoveIdentityAsync(string externalIdentity, CancellationToken cancellationToken)
    {
        var user = await api.GetUserAsync(externalIdentity, cancellationToken);

        if (user.Data is null)
        {
            return;
        }

        await api.DeleteUserAsync(user.Data with { Password = null }, cancellationToken);
    }

    public async Task<bool> IsInitialPasswordPendingAsync(string externalIdentity, CancellationToken cancellationToken)
    {
        var user = await api.GetUserAsync(externalIdentity, cancellationToken);

        return user.Data?.NeedUpdatePassword ?? true;
    }
}
