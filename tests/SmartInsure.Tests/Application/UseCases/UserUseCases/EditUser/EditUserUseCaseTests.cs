using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using SmartInsure.Application.UseCase.Services.Invitations;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.EditUser;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.EditUser.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Exceptions;
using SmartInsure.Infra.CrossCutting.Options;
using Xunit;

namespace SmartInsure.Tests.Application.UseCases.UserUseCases.EditUser;

[Trait("Category", "UseCase")]
[Trait("RuleId", "RN-202")]
public sealed class EditUserUseCaseTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IInvitationRepository _invitationRepository = Substitute.For<IInvitationRepository>();
    private readonly IIdentityProvider _identityProvider = Substitute.For<IIdentityProvider>();
    private readonly IInvitationMailer _invitationMailer = Substitute.For<IInvitationMailer>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly EditUserUseCase _useCase;

    public EditUserUseCaseTests()
        => _useCase = new EditUserUseCase(
            _userRepository,
            _invitationRepository,
            _identityProvider,
            _invitationMailer,
            _unitOfWork,
            Options.Create(new InvitationOptions { LinkExpiryDays = 7 }),
            Substitute.For<ILogger<EditUserUseCase>>());

    private User Arrange(string email = "antigo@corretora.com.br")
    {
        var user = User.Create("Nome Antigo", email, "casdoor-1");
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        return user;
    }

    [Fact]
    public async Task Execute_DeveRenomear_SemTocarNoEmailNemNoProvedor_QuandoSomenteNome()
    {
        var user = Arrange();

        var result = await _useCase.ExecuteAsync(
            new EditUserRequest(user.Id, "Nome Novo", null), CancellationToken.None);

        result.Name.Should().Be("Nome Novo");
        result.Email.Should().Be("antigo@corretora.com.br");
        await _identityProvider.DidNotReceiveWithAnyArgs().UpdateEmailAsync(default!, default!, default);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveTrocarEmailNoProvedorEReenviarConvite_QuandoPendente()
    {
        var user = Arrange();
        _userRepository.EmailExistsAsync("novo@corretora.com.br", Arg.Any<CancellationToken>()).Returns(false);
        _identityProvider.EmailExistsAsync("novo@corretora.com.br", Arg.Any<CancellationToken>()).Returns(false);

        var result = await _useCase.ExecuteAsync(
            new EditUserRequest(user.Id, "Nome Novo", "novo@corretora.com.br"), CancellationToken.None);

        result.Email.Should().Be("novo@corretora.com.br");
        result.Status.Should().Be("Pending");
        await _identityProvider.Received(1).UpdateEmailAsync(
            "casdoor-1", "novo@corretora.com.br", Arg.Any<CancellationToken>());
        await _invitationRepository.Received(1).AddAsync(Arg.Any<Invitation>(), Arg.Any<CancellationToken>());
        await _invitationMailer.Received(1).SendAsync(
            "novo@corretora.com.br", Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRecusarTrocaDeEmail_QuandoUsuarioAtivo()
    {
        var user = Arrange();
        user.Activate();

        var act = async () => await _useCase.ExecuteAsync(
            new EditUserRequest(user.Id, "Nome Novo", "novo@corretora.com.br"), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        await _identityProvider.DidNotReceiveWithAnyArgs().UpdateEmailAsync(default!, default!, default);
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoEmailNovoJaExiste()
    {
        var user = Arrange();
        _userRepository.EmailExistsAsync("novo@corretora.com.br", Arg.Any<CancellationToken>()).Returns(true);

        var act = async () => await _useCase.ExecuteAsync(
            new EditUserRequest(user.Id, "Nome Novo", "novo@corretora.com.br"), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        await _identityProvider.DidNotReceiveWithAnyArgs().UpdateEmailAsync(default!, default!, default);
    }

    [Fact]
    public async Task Execute_DeveLancarNotFound_QuandoUsuarioNaoExiste()
    {
        var id = Guid.NewGuid();
        _userRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((User?)null);

        var act = async () => await _useCase.ExecuteAsync(
            new EditUserRequest(id, "Nome", null), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
