using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageAdministrator;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.InviteBrokerageAdministrator.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Abstractions.Services.Dtos;
using SmartInsure.Core.Constants;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using SmartInsure.Infra.CrossCutting.Options;

namespace SmartInsure.Tests.Application.UseCases.UserUseCases.InviteBrokerageAdministrator;

/// <summary>RN-036 — o Administrador do Sistema convida Corretor Administrador para Corretoras.</summary>
[Trait("RuleId", "RN-036")]
public class InviteBrokerageAdministratorUseCaseTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPersonRepository _personRepository = Substitute.For<IPersonRepository>();
    private readonly IProfileRepository _profileRepository = Substitute.For<IProfileRepository>();
    private readonly IUserBrokerageMembershipRepository _membershipRepository =
        Substitute.For<IUserBrokerageMembershipRepository>();
    private readonly IInvitationRepository _invitationRepository = Substitute.For<IInvitationRepository>();
    private readonly IIdentityProvider _identityProvider = Substitute.For<IIdentityProvider>();
    private readonly IMailService _mailService = Substitute.For<IMailService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly Profile _brokerageAdministrator =
        Profile.Create(ProfileNames.BrokerageAdministrator, EProfileScope.Brokerage, isFixed: true);

    private readonly InviteBrokerageAdministratorUseCase _useCase;

    public InviteBrokerageAdministratorUseCaseTests()
    {
        var options = Options.Create(new InvitationOptions
        {
            AppBaseUrl = "https://app.example.com",
            LinkExpiryDays = 7,
        });

        _useCase = new InviteBrokerageAdministratorUseCase(
            _userRepository, _personRepository, _profileRepository, _membershipRepository,
            _invitationRepository, _identityProvider, _mailService, _unitOfWork, options,
            Substitute.For<ILogger<InviteBrokerageAdministratorUseCase>>());

        _profileRepository.GetBrokerageAdministratorAsync(Arg.Any<CancellationToken>())
            .Returns(_brokerageAdministrator);
        _identityProvider.CreateIdentityAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("casdoor-ca-1");
        _mailService.SendAsync(Arg.Any<MailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
    }

    private static Person Brokerage(bool active = true)
    {
        var person = Person.Create("11222333000181", "Corretora X", null, Guid.CreateVersion7());
        person.AssignRole(EPersonRole.Broker);
        if (!active)
        {
            person.GetRole(EPersonRole.Broker)!.Deactivate();
        }

        return person;
    }

    private static InviteBrokerageAdministratorRequest Request(params Guid[] brokerageIds)
        => new("Maria Silva", "maria@corretora.com.br", brokerageIds);

    [Fact]
    public async Task Execute_DeveConvidarCAComVinculos_QuandoDadosValidos()
    {
        _personRepository.GetTrackedBrokerageByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Brokerage());

        var response = await _useCase.ExecuteAsync(
            Request(Guid.CreateVersion7(), Guid.CreateVersion7()), CancellationToken.None);

        response.Status.Should().Be("Pending");
        await _userRepository.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _invitationRepository.Received(1).AddAsync(Arg.Any<Invitation>(), Arg.Any<CancellationToken>());
        // RN-036: um vínculo Corretor Administrador por Corretora informada.
        await _membershipRepository.Received(2).AddAsync(
            Arg.Is<UserBrokerageMembership>(membership => membership.ProfileId == _brokerageAdministrator.Id),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
        await _mailService.Received(1).SendAsync(Arg.Any<MailMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoCorretoraInativa()
    {
        _personRepository.GetTrackedBrokerageByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Brokerage(active: false));

        var act = () => _useCase.ExecuteAsync(Request(Guid.CreateVersion7()), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        await _identityProvider.DidNotReceiveWithAnyArgs().CreateIdentityAsync(default!, default!, default);
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoCorretoraInexistente()
    {
        _personRepository.GetTrackedBrokerageByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Person?)null);

        var act = () => _useCase.ExecuteAsync(Request(Guid.CreateVersion7()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Execute_DeveRecusarSemCriarIdentidade_QuandoEmailJaExisteNaPlataforma()
    {
        _userRepository.EmailExistsAsync("maria@corretora.com.br", Arg.Any<CancellationToken>())
            .Returns(true);

        var act = () => _useCase.ExecuteAsync(Request(Guid.CreateVersion7()), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        await _identityProvider.DidNotReceiveWithAnyArgs().CreateIdentityAsync(default!, default!, default);
    }
}
