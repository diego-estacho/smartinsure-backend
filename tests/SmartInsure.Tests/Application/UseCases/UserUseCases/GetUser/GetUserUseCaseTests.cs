using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.GetUser;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.GetUser.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Exceptions;
using Xunit;

namespace SmartInsure.Tests.Application.UseCases.UserUseCases.GetUser;

[Trait("Category", "UseCase")]
public sealed class GetUserUseCaseTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();

    [Fact]
    [Trait("RuleId", "RN-064")]
    public async Task Execute_DeveTrazerVinculosDeCorretoraEDeTomador()
    {
        var userId = Guid.NewGuid();
        var brokerageMembership = new UserMembershipDto(
            Guid.NewGuid(), Guid.NewGuid(), "11222333000181", "Corretora Alfa",
            Guid.NewGuid(), "BrokerageAdministrator", "Brokerage", true);
        var policyHolderMembership = new UserMembershipDto(
            Guid.NewGuid(), Guid.NewGuid(), "99888777000166", "Tomador Beta",
            Guid.NewGuid(), "PolicyHolderAdministrator", "PolicyHolder", true);
        _userRepository.GetDetailsByIdAsync(userId, CancellationToken.None)
            .Returns(new UserDetailsDto(
                userId, "Ana", "ana@exemplo.com", "52998224725", "Active", null, null, null, false, DateTime.UtcNow,
                null, null, false, null, [brokerageMembership], [policyHolderMembership]));

        var useCase = new GetUserUseCase(_userRepository);
        var result = await useCase.ExecuteAsync(new GetUserRequest(userId), CancellationToken.None);

        result.BrokerageMemberships.Should().HaveCount(1);
        result.BrokerageMemberships[0].ScopeName.Should().Be("Corretora Alfa");
        result.BrokerageMemberships[0].ProfileName.Should().Be("BrokerageAdministrator");
        result.BrokerageMemberships[0].ProfileScope.Should().Be("Brokerage");
        result.PolicyHolderMemberships.Should().HaveCount(1);
        result.PolicyHolderMemberships[0].ScopeName.Should().Be("Tomador Beta");
        result.DocumentNumber.Should().Be("52998224725");
    }

    [Fact]
    [Trait("RuleId", "RN-012")]
    public async Task Execute_DeveIndicarUsuarioSemPerfil()
    {
        var userId = Guid.NewGuid();
        _userRepository.GetDetailsByIdAsync(userId, CancellationToken.None)
            .Returns(new UserDetailsDto(
                userId, "Bruno", "bruno@exemplo.com", null, "Pending", null, null, null, false, DateTime.UtcNow,
                null, null, false, null, [], []));

        var useCase = new GetUserUseCase(_userRepository);
        var result = await useCase.ExecuteAsync(new GetUserRequest(userId), CancellationToken.None);

        result.ProfileId.Should().BeNull();
        result.ProfileName.Should().BeNull();
        result.Status.Should().Be("Pending");
    }

    [Fact]
    [Trait("RuleId", "RN-065")]
    public async Task Execute_DeveTrazerDadosDoConvite_QuandoPendenteComConviteVencido()
    {
        var userId = Guid.NewGuid();
        _userRepository.GetDetailsByIdAsync(userId, CancellationToken.None)
            .Returns(new UserDetailsDto(
                userId, "Bruno", "bruno@exemplo.com", null, "Pending", null, null, null, false, DateTime.UtcNow,
                DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(-3), true, null, [], []));

        var useCase = new GetUserUseCase(_userRepository);
        var result = await useCase.ExecuteAsync(new GetUserRequest(userId), CancellationToken.None);

        result.InviteExpired.Should().BeTrue();
        result.InvitedAt.Should().NotBeNull();
        result.InviteExpiresAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Execute_DeveLancarNotFound_QuandoUsuarioNaoExiste()
    {
        var userId = Guid.NewGuid();
        _userRepository.GetDetailsByIdAsync(userId, CancellationToken.None)
            .Returns((UserDetailsDto?)null);

        var useCase = new GetUserUseCase(_userRepository);

        var act = async () => await useCase.ExecuteAsync(
            new GetUserRequest(userId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
