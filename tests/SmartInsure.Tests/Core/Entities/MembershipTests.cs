using FluentAssertions;
using SmartInsure.Core.Entities;

namespace SmartInsure.Tests.Core.Entities;

/// <summary>RN-034 — vínculos do Usuário com Corretora e Tomador (um Perfil por vínculo).</summary>
[Trait("RuleId", "RN-034")]
public class MembershipTests
{
    [Fact]
    public void CreateBrokerageMembership_DeveGuardarUsuarioCorretoraEPerfil()
    {
        var userId = Guid.CreateVersion7();
        var brokerageId = Guid.CreateVersion7();
        var profileId = Guid.CreateVersion7();

        var membership = UserBrokerageMembership.Create(userId, brokerageId, profileId);

        membership.UserId.Should().Be(userId);
        membership.BrokerageId.Should().Be(brokerageId);
        membership.ProfileId.Should().Be(profileId);
    }

    [Fact]
    public void CreatePolicyHolderMembership_DeveGuardarUsuarioTomadorEPerfil()
    {
        var userId = Guid.CreateVersion7();
        var policyHolderId = Guid.CreateVersion7();
        var profileId = Guid.CreateVersion7();

        var membership = UserPolicyHolderMembership.Create(userId, policyHolderId, profileId);

        membership.UserId.Should().Be(userId);
        membership.PolicyHolderId.Should().Be(policyHolderId);
        membership.ProfileId.Should().Be(profileId);
    }
}
