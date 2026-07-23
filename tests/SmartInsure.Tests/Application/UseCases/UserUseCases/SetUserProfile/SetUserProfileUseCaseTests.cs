using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.SetUserProfile;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.SetUserProfile.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Constants;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.UserUseCases.SetUserProfile;

/// <summary>RN-012 — concessão/revogação do Perfil Administrador do Sistema (agora entidade — RN-032).</summary>
[Trait("RuleId", "RN-012")]
public class SetUserProfileUseCaseTests
{
    private readonly IUserRepository _repository = Substitute.For<IUserRepository>();
    private readonly IProfileRepository _profileRepository = Substitute.For<IProfileRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IDistributedCache _cache = Substitute.For<IDistributedCache>();
    private readonly SetUserProfileUseCase _useCase;

    private readonly Profile _systemAdministrator =
        Profile.Create(ProfileNames.SystemAdministrator, EProfileScope.System, isFixed: true);

    public SetUserProfileUseCaseTests()
    {
        _useCase = new SetUserProfileUseCase(_repository, _profileRepository, _unitOfWork, _cache);

        _profileRepository.GetByNameAsync(ProfileNames.SystemAdministrator, Arg.Any<CancellationToken>())
            .Returns(_systemAdministrator);
        _profileRepository.GetSystemAdministratorAsync(Arg.Any<CancellationToken>())
            .Returns(_systemAdministrator);
    }

    private User ExistingUser(bool admin = false)
    {
        var user = User.Create("Maria Silva", "maria@corretora.com.br", "casdoor-id-123");
        if (admin)
        {
            user.GrantProfile(_systemAdministrator);
        }

        _repository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        return user;
    }

    [Fact]
    public async Task Execute_DeveConcederPerfil_QuandoUsuarioComum()
    {
        var user = ExistingUser();

        var response = await _useCase.ExecuteAsync(
            new SetUserProfileRequest(user.Id, ProfileNames.SystemAdministrator), CancellationToken.None);

        response.Profile.Should().Be(ProfileNames.SystemAdministrator);
        user.ProfileId.Should().Be(_systemAdministrator.Id);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
        await _cache.Received(1).RemoveAsync("user-profile:casdoor-id-123", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRevogarPerfil_QuandoHaOutroAdministrador()
    {
        var user = ExistingUser(admin: true);
        _repository.CountByProfileIdAsync(_systemAdministrator.Id, Arg.Any<CancellationToken>())
            .Returns(2);

        var response = await _useCase.ExecuteAsync(
            new SetUserProfileRequest(user.Id, null), CancellationToken.None);

        response.Profile.Should().BeNull();
        user.ProfileId.Should().BeNull();
        await _cache.Received(1).RemoveAsync("user-profile:casdoor-id-123", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRecusarRevogacao_QuandoUsuarioEOUltimoAdministrador()
    {
        var user = ExistingUser(admin: true);
        _repository.CountByProfileIdAsync(_systemAdministrator.Id, Arg.Any<CancellationToken>())
            .Returns(1);

        var act = () => _useCase.ExecuteAsync(
            new SetUserProfileRequest(user.Id, null), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        user.ProfileId.Should().Be(_systemAdministrator.Id);
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    public async Task Execute_DeveLancarNotFound_QuandoUsuarioInexistente()
    {
        var act = () => _useCase.ExecuteAsync(
            new SetUserProfileRequest(Guid.NewGuid(), ProfileNames.SystemAdministrator), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoPerfilDesconhecido()
    {
        var user = ExistingUser();
        _profileRepository.GetByNameAsync("SuperUser", Arg.Any<CancellationToken>())
            .Returns((Profile?)null);

        var act = () => _useCase.ExecuteAsync(
            new SetUserProfileRequest(user.Id, "SuperUser"), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }
}
