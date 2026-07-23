using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ChangeUserActivation;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.ChangeUserActivation.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Constants;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.UserUseCases.ChangeUserActivation;

/// <summary>RN-046 — inativação/reativação de Usuário (Administrador do Sistema).</summary>
[Trait("RuleId", "RN-046")]
public class ChangeUserActivationUseCaseTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IProfileRepository _profileRepository = Substitute.For<IProfileRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly Profile _systemAdministrator =
        Profile.Create(ProfileNames.SystemAdministrator, EProfileScope.System, isFixed: true);

    private readonly ChangeUserActivationUseCase _useCase;

    public ChangeUserActivationUseCaseTests()
    {
        _useCase = new ChangeUserActivationUseCase(_userRepository, _profileRepository, _unitOfWork);
        _profileRepository.GetSystemAdministratorAsync(Arg.Any<CancellationToken>())
            .Returns(_systemAdministrator);
    }

    private User ActiveUser(bool administrator = false)
    {
        var user = User.Create("Maria", "maria@corretora.com.br", "casdoor-1");
        if (administrator)
        {
            user.GrantProfile(_systemAdministrator);
        }

        user.Activate();
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        return user;
    }

    [Fact]
    public async Task Execute_DeveInativar_QuandoUsuarioComumAtivo()
    {
        var user = ActiveUser();

        var response = await _useCase.ExecuteAsync(
            new ChangeUserActivationRequest(user.Id, Activate: false), CancellationToken.None);

        response.Status.Should().Be("Inactive");
        user.Status.Should().Be(EUserStatus.Inactive);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveReativar_QuandoUsuarioInativo()
    {
        var user = ActiveUser();
        user.Deactivate();

        var response = await _useCase.ExecuteAsync(
            new ChangeUserActivationRequest(user.Id, Activate: true), CancellationToken.None);

        response.Status.Should().Be("Active");
        user.Status.Should().Be(EUserStatus.Active);
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoInativaOUltimoAdministrador()
    {
        var user = ActiveUser(administrator: true);
        _userRepository.CountByProfileIdAsync(_systemAdministrator.Id, Arg.Any<CancellationToken>())
            .Returns(1);

        var act = () => _useCase.ExecuteAsync(
            new ChangeUserActivationRequest(user.Id, Activate: false), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        user.Status.Should().Be(EUserStatus.Active);
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    public async Task Execute_DevePermitirInativarAdministrador_QuandoHaOutro()
    {
        var user = ActiveUser(administrator: true);
        _userRepository.CountByProfileIdAsync(_systemAdministrator.Id, Arg.Any<CancellationToken>())
            .Returns(2);

        var response = await _useCase.ExecuteAsync(
            new ChangeUserActivationRequest(user.Id, Activate: false), CancellationToken.None);

        response.Status.Should().Be("Inactive");
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoInativaUsuarioJaInativo()
    {
        var user = ActiveUser();
        user.Deactivate();

        var act = () => _useCase.ExecuteAsync(
            new ChangeUserActivationRequest(user.Id, Activate: false), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Execute_DeveLancarNotFound_QuandoUsuarioInexistente()
    {
        var act = () => _useCase.ExecuteAsync(
            new ChangeUserActivationRequest(Guid.CreateVersion7(), Activate: false), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
