using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using SmartInsure.Api.Services;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Constants;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Tests.Api.Services;

// RN-011 — o Perfil vira role; usa a entidade Profile (RN-032).

/// <summary>RN-011 — o Perfil do Usuário vira role nas claims; sem perfil, nenhuma role.</summary>
[Trait("RuleId", "RN-011")]
public class UserProfileClaimsTransformationTests
{
    private readonly IUserRepository _repository = Substitute.For<IUserRepository>();
    private readonly IDistributedCache _cache = Substitute.For<IDistributedCache>();
    private readonly UserProfileClaimsTransformation _transformation;

    public UserProfileClaimsTransformationTests()
    {
        // NSubstitute retorna byte[] vazio (não null) por padrão para membros Task<byte[]?>
        // não configurados; um IDistributedCache real retorna null em cache miss.
        _cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((byte[]?)null);
        _transformation = new UserProfileClaimsTransformation(_repository, _cache);
    }

    private static ClaimsPrincipal Principal(string externalIdentity)
        => new(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, externalIdentity)], "TestAuth"));

    private void UserWithProfile(string externalIdentity, bool admin)
    {
        var user = User.Create("Maria Silva", "maria@corretora.com.br", externalIdentity);
        if (admin)
        {
            user.GrantProfile(
                Profile.Create(ProfileNames.SystemAdministrator, EProfileScope.System, isFixed: true));
        }

        _repository.GetByExternalIdentityAsync(externalIdentity, Arg.Any<CancellationToken>())
            .Returns(user);
    }

    [Fact]
    public async Task Transform_DeveAdicionarRole_QuandoUsuarioEAdministradorDoSistema()
    {
        UserWithProfile("casdoor-id-123", admin: true);

        var principal = await _transformation.TransformAsync(Principal("casdoor-id-123"));

        principal.IsInRole(Roles.SystemAdministrator).Should().BeTrue();
    }

    [Fact]
    public async Task Transform_NaoDeveAdicionarRole_QuandoUsuarioComum()
    {
        UserWithProfile("casdoor-id-123", admin: false);

        var principal = await _transformation.TransformAsync(Principal("casdoor-id-123"));

        principal.IsInRole(Roles.SystemAdministrator).Should().BeFalse();
    }

    [Fact]
    public async Task Transform_NaoDeveConsultarRepositorio_QuandoPerfilEmCache()
    {
        _cache.GetAsync("user-profile:casdoor-id-123", Arg.Any<CancellationToken>())
            .Returns(System.Text.Encoding.UTF8.GetBytes("SystemAdministrator"));

        var principal = await _transformation.TransformAsync(Principal("casdoor-id-123"));

        principal.IsInRole(Roles.SystemAdministrator).Should().BeTrue();
        await _repository.DidNotReceiveWithAnyArgs().GetByExternalIdentityAsync(default!, default);
    }
}
