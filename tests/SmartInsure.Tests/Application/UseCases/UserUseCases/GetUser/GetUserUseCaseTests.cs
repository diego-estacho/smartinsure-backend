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
            Guid.NewGuid(), "BrokerageAdministrator");
        var policyHolderMembership = new UserMembershipDto(
            Guid.NewGuid(), Guid.NewGuid(), "99888777000166", "Tomador Beta",
            Guid.NewGuid(), "PolicyHolderAdministrator");
        _userRepository.GetDetailsByIdAsync(userId, CancellationToken.None)
            .Returns(new UserDetailsDto(
                userId, "Ana", "ana@exemplo.com", "Active", null, null, DateTime.UtcNow,
                [brokerageMembership], [policyHolderMembership]));

        var useCase = new GetUserUseCase(_userRepository);
        var result = await useCase.ExecuteAsync(new GetUserRequest(userId), CancellationToken.None);

        result.BrokerageMemberships.Should().HaveCount(1);
        result.BrokerageMemberships[0].ScopeName.Should().Be("Corretora Alfa");
        result.BrokerageMemberships[0].ProfileName.Should().Be("BrokerageAdministrator");
        result.PolicyHolderMemberships.Should().HaveCount(1);
        result.PolicyHolderMemberships[0].ScopeName.Should().Be("Tomador Beta");
    }

    [Fact]
    [Trait("RuleId", "RN-012")]
    public async Task Execute_DeveIndicarUsuarioSemPerfil()
    {
        var userId = Guid.NewGuid();
        _userRepository.GetDetailsByIdAsync(userId, CancellationToken.None)
            .Returns(new UserDetailsDto(
                userId, "Bruno", "bruno@exemplo.com", "Pending", null, null, DateTime.UtcNow, [], []));

        var useCase = new GetUserUseCase(_userRepository);
        var result = await useCase.ExecuteAsync(new GetUserRequest(userId), CancellationToken.None);

        result.ProfileId.Should().BeNull();
        result.ProfileName.Should().BeNull();
        result.Status.Should().Be("Pending");
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
