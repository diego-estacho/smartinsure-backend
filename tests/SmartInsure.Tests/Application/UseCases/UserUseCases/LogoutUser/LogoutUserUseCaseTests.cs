using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.LogoutUser;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.LogoutUser.Requests;
using SmartInsure.Core.Abstractions.Services;

namespace SmartInsure.Tests.Application.UseCases.UserUseCases.LogoutUser;

/// <summary>RN-006 — Encerramento de sessão.</summary>
[Trait("RuleId", "RN-006")]
public class LogoutUserUseCaseTests
{
    private static readonly DateTime ExpiresAtUtc = new(2026, 7, 16, 20, 0, 0, DateTimeKind.Utc);

    private readonly IAccessTokenRevocationStore _revocationStore =
        Substitute.For<IAccessTokenRevocationStore>();

    private readonly LogoutUserUseCase _useCase;

    public LogoutUserUseCaseTests()
        => _useCase = new LogoutUserUseCase(_revocationStore);

    [Fact]
    public async Task Execute_DeveRegistrarAcessoNaDenylist_QuandoSessaoValida()
    {
        await _useCase.ExecuteAsync(
            new LogoutUserRequest("jti-123", ExpiresAtUtc), CancellationToken.None);

        await _revocationStore.Received(1)
            .RevokeAsync("jti-123", ExpiresAtUtc, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveSerNoOp_QuandoAcessoSemIdentificador()
    {
        await _useCase.ExecuteAsync(
            new LogoutUserRequest(null, ExpiresAtUtc), CancellationToken.None);

        await _revocationStore.DidNotReceiveWithAnyArgs()
            .RevokeAsync(default!, default, default);
    }

    [Fact]
    public async Task Execute_DeveSerNoOp_QuandoAcessoSemExpiracao()
    {
        await _useCase.ExecuteAsync(
            new LogoutUserRequest("jti-123", null), CancellationToken.None);

        await _revocationStore.DidNotReceiveWithAnyArgs()
            .RevokeAsync(default!, default, default);
    }
}
