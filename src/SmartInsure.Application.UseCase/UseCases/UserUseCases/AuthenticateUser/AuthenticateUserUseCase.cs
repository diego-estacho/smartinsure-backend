using SmartInsure.Application.UseCase.UseCases.UserUseCases.AuthenticateUser.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.AuthenticateUser.Requests;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.AuthenticateUser.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.UseCases.UserUseCases.AuthenticateUser;

/// <summary>
/// RN-005 — Autenticação de Usuário com e-mail e senha: credenciais validadas
/// exclusivamente no provedor de identidade; somente Usuário Ativo recebe acesso
/// autenticado, válido por 8 horas. Recusa por credencial usa mensagem única que não
/// revela se o e-mail está cadastrado.
/// </summary>
public sealed class AuthenticateUserUseCase(
    IUserRepository userRepository,
    IIdentityProvider identityProvider,
    IAccessTokenIssuer accessTokenIssuer) : IAuthenticateUserUseCase
{
    internal const string InvalidCredentialsMessage = "E-mail ou senha inválidos.";

    /// <summary>Executa o caso de uso de autenticação de usuário.</summary>
    public async Task<AuthenticateUserResponse> ExecuteAsync(
        AuthenticateUserRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await userRepository.GetByEmailAsync(email, cancellationToken)
            ?? throw new UnauthorizedException(InvalidCredentialsMessage);

        if (!await identityProvider.ValidateCredentialsAsync(email, request.Password, cancellationToken))
        {
            throw new UnauthorizedException(InvalidCredentialsMessage);
        }

        // RN-005/RN-046: só o Usuário Ativo acessa. Credencial já validada: a recusa é regra de
        // negócio (422), não falha de autenticação (ADR-012). A situação só é revelada a quem provou
        // conhecer a senha.
        if (user.Status == EUserStatus.Inactive)
        {
            throw new BusinessRuleException(
                "Usuário inativo. Procure um administrador para reativar o acesso.");
        }

        if (user.Status != EUserStatus.Active)
        {
            throw new BusinessRuleException(
                "Usuário pendente do primeiro acesso. Conclua o primeiro acesso para entrar na plataforma.");
        }

        var accessToken = accessTokenIssuer.IssueFor(user);

        return new AuthenticateUserResponse(accessToken.Token, accessToken.ExpiresAtUtc);
    }
}
