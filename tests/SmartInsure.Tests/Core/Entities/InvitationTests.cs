using FluentAssertions;
using SmartInsure.Core.Entities;

namespace SmartInsure.Tests.Core.Entities;

/// <summary>RN-035 — convite de primeiro acesso: token único, validade, reenviável.</summary>
[Trait("RuleId", "RN-035")]
public class InvitationTests
{
    [Fact]
    public void Create_DeveGerarTokenEGuardarHash()
    {
        var userId = Guid.CreateVersion7();

        var (invitation, plainToken) = Invitation.Create(userId, 7);

        invitation.UserId.Should().Be(userId);
        invitation.TokenHash.Should().NotBeNullOrEmpty().And.HaveLength(64); // SHA256 em hex
        plainToken.Should().NotBeNullOrEmpty();
        invitation.ExpiresAtUtc.Should().BeAfter(DateTime.UtcNow);
        invitation.ConsumedAtUtc.Should().BeNull();
    }

    [Fact]
    public void IsValid_DeveSerFalso_QuandoConsumido()
    {
        var (invitation, _) = Invitation.Create(Guid.CreateVersion7(), 7);
        invitation.Consume();

        invitation.IsValid().Should().BeFalse();
    }

    [Fact]
    public void IsValid_DeveSerFalso_QuandoExpirado()
    {
        var (invitation, _) = Invitation.Create(Guid.CreateVersion7(), -1); // -1 dia = já expirado

        invitation.IsValid().Should().BeFalse();
    }

    [Fact]
    public void Consume_DeveMarcarComoConsumido()
    {
        var (invitation, _) = Invitation.Create(Guid.CreateVersion7(), 7);

        invitation.Consume();

        invitation.ConsumedAtUtc.Should().NotBeNull().And.BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
