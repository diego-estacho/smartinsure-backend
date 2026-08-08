using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ChangeUserScopeProfile;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ChangeUserScopeProfile.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using Xunit;

namespace SmartInsure.Tests.Application.UseCases.UserUseCases.ChangeUserScopeProfile;

[Trait("Category", "UseCase")]
[Trait("RuleId", "RN-075")]
public sealed class ChangeUserScopeProfileUseCaseTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUserBrokerageMembershipRepository _brokerageMembershipRepository
        = Substitute.For<IUserBrokerageMembershipRepository>();
    private readonly IUserPolicyHolderMembershipRepository _policyHolderMembershipRepository
        = Substitute.For<IUserPolicyHolderMembershipRepository>();
    private readonly IProfileRepository _profileRepository = Substitute.For<IProfileRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IDistributedCache _cache = Substitute.For<IDistributedCache>();
    private readonly ChangeUserScopeProfileUseCase _useCase;

    private readonly User _user = User.Create("Carla", "carla@corretora.com.br", "casdoor-1");
    private readonly Guid _brokerageId = Guid.NewGuid();

    public ChangeUserScopeProfileUseCaseTests()
    {
        _useCase = new ChangeUserScopeProfileUseCase(
            _userRepository,
            _brokerageMembershipRepository,
            _policyHolderMembershipRepository,
            _profileRepository,
            _unitOfWork,
            _cache);
        _userRepository.GetByIdAsync(_user.Id, Arg.Any<CancellationToken>()).Returns(_user);
    }

    [Fact]
    public async Task Execute_DeveTrocarPerfilNoVinculoDeCorretora_QuandoPerfilDoMesmoEscopo()
    {
        var profile = Profile.CreateForBrokerage("Operação de emissão", _brokerageId);
        _profileRepository.GetByIdAsync(profile.Id, Arg.Any<CancellationToken>()).Returns(profile);
        var membership = UserBrokerageMembership.Create(_user.Id, _brokerageId, Guid.NewGuid());
        _brokerageMembershipRepository
            .GetByUserAndBrokerageAsync(_user.Id, _brokerageId, Arg.Any<CancellationToken>())
            .Returns(membership);

        var result = await _useCase.ExecuteAsync(
            new ChangeUserScopeProfileRequest(_user.Id, _brokerageId, profile.Id), CancellationToken.None);

        membership.ProfileId.Should().Be(profile.Id);
        result.ProfileId.Should().Be(profile.Id);
        _brokerageMembershipRepository.Received(1).Update(membership);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
        await _cache.Received(1).RemoveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoPerfilDeOutroEscopo()
    {
        var profile = Profile.Create("Administrador do Sistema", EProfileScope.System, isFixed: true);
        _profileRepository.GetByIdAsync(profile.Id, Arg.Any<CancellationToken>()).Returns(profile);
        _brokerageMembershipRepository
            .GetByUserAndBrokerageAsync(_user.Id, _brokerageId, Arg.Any<CancellationToken>())
            .Returns(UserBrokerageMembership.Create(_user.Id, _brokerageId, Guid.NewGuid()));

        var act = async () => await _useCase.ExecuteAsync(
            new ChangeUserScopeProfileRequest(_user.Id, _brokerageId, profile.Id), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Execute_DeveLancarNotFound_QuandoNaoHaVinculoNoEscopo()
    {
        var profile = Profile.CreateForBrokerage("Comercial", _brokerageId);
        _profileRepository.GetByIdAsync(profile.Id, Arg.Any<CancellationToken>()).Returns(profile);
        _brokerageMembershipRepository
            .GetByUserAndBrokerageAsync(_user.Id, _brokerageId, Arg.Any<CancellationToken>())
            .Returns((UserBrokerageMembership?)null);
        _policyHolderMembershipRepository
            .GetByUserAndPolicyHolderAsync(_user.Id, _brokerageId, Arg.Any<CancellationToken>())
            .Returns((UserPolicyHolderMembership?)null);

        var act = async () => await _useCase.ExecuteAsync(
            new ChangeUserScopeProfileRequest(_user.Id, _brokerageId, profile.Id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
