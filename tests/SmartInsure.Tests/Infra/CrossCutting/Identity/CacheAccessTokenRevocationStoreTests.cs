using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SmartInsure.Infra.CrossCutting.Identity;

namespace SmartInsure.Tests.Infra.CrossCutting.Identity;

/// <summary>RN-006 — denylist de acessos encerrados.</summary>
[Trait("RuleId", "RN-006")]
public class CacheAccessTokenRevocationStoreTests
{
    private readonly CacheAccessTokenRevocationStore _store = new(
        new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions())));

    [Fact]
    public async Task IsRevoked_DeveSerVerdadeiro_QuandoAcessoEncerrado()
    {
        await _store.RevokeAsync(
            "jti-123", DateTime.UtcNow.AddHours(8), CancellationToken.None);

        (await _store.IsRevokedAsync("jti-123", CancellationToken.None)).Should().BeTrue();
    }

    [Fact]
    public async Task IsRevoked_DeveSerFalso_QuandoAcessoNaoEncerrado()
    {
        (await _store.IsRevokedAsync("jti-desconhecido", CancellationToken.None))
            .Should().BeFalse();
    }

    [Fact]
    public async Task Revoke_DeveSerNoOp_QuandoAcessoJaExpirado()
    {
        await _store.RevokeAsync(
            "jti-expirado", DateTime.UtcNow.AddMinutes(-1), CancellationToken.None);

        (await _store.IsRevokedAsync("jti-expirado", CancellationToken.None))
            .Should().BeFalse();
    }

    [Fact]
    public async Task Revoke_DeveSerIdempotente_QuandoEncerradoDuasVezes()
    {
        var expiresAt = DateTime.UtcNow.AddHours(1);

        await _store.RevokeAsync("jti-123", expiresAt, CancellationToken.None);
        await _store.RevokeAsync("jti-123", expiresAt, CancellationToken.None);

        (await _store.IsRevokedAsync("jti-123", CancellationToken.None)).Should().BeTrue();
    }
}
