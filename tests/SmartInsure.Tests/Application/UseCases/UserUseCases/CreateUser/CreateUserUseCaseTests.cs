using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.CreateUser;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.CreateUser.Interfaces;
using SmartInsure.Application.UseCase.UseCases.UserUseCases.CreateUser.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Exceptions;
using SmartInsure.Infra.CrossCutting.Options;

namespace SmartInsure.Tests.Application.UseCases.UserUseCases.CreateUser;

/// <summary>RN-001/RN-035 — Criação de Usuário + Convite.</summary>
[Trait("RuleId", "RN-001")]
[Trait("RuleId", "RN-035")]
public class CreateUserUseCaseTests
{
    private readonly IUserRepository _repository = Substitute.For<IUserRepository>();
    private readonly IInvitationRepository _invitationRepository = Substitute.For<IInvitationRepository>();
    private readonly IIdentityProvider _identityProvider = Substitute.For<IIdentityProvider>();
    private readonly IMailService _mailService = Substitute.For<IMailService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ILogger<CreateUserUseCase> _logger = Substitute.For<ILogger<CreateUserUseCase>>();
    private readonly CreateUserUseCase _useCase;

    public CreateUserUseCaseTests()
    {
        var options = Options.Create(new InvitationOptions
        {
            AppBaseUrl = "https://app.example.com",
            LinkExpiryDays = 7,
        });

        _useCase = new CreateUserUseCase(
            _repository, _invitationRepository, _identityProvider, _mailService, _unitOfWork, options, _logger);

        _mailService.SendAsync(Arg.Any<SmartInsure.Core.Abstractions.Services.Dtos.MailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
    }

    private static CreateUserRequest Request()
        => new("Maria Silva", "maria.silva@corretora.com.br");

    [Fact]
    public async Task Execute_DeveCriarUsuarioPendenteComIdentidadeExterna_QuandoDadosValidos()
    {
        _identityProvider.CreateIdentityAsync("Maria Silva", "maria.silva@corretora.com.br", Arg.Any<CancellationToken>())
            .Returns("casdoor-id-123");

        var response = await _useCase.ExecuteAsync(Request(), CancellationToken.None);

        response.Status.Should().Be("Pending");
        response.Email.Should().Be("maria.silva@corretora.com.br");

        await _repository.Received(1).AddAsync(
            Arg.Is<User>(user => user.ExternalIdentity == "casdoor-id-123"),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRecusarSemCriarIdentidade_QuandoEmailJaExisteNaPlataforma()
    {
        _repository.EmailExistsAsync("maria.silva@corretora.com.br", Arg.Any<CancellationToken>())
            .Returns(true);

        var act = () => _useCase.ExecuteAsync(Request(), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        await _identityProvider.DidNotReceiveWithAnyArgs()
            .CreateIdentityAsync(default!, default!, default);
    }

    [Fact]
    public async Task Execute_DeveRecusarSemAdotarIdentidade_QuandoEmailJaExisteNoProvedor()
    {
        _identityProvider.EmailExistsAsync("maria.silva@corretora.com.br", Arg.Any<CancellationToken>())
            .Returns(true);

        var act = () => _useCase.ExecuteAsync(Request(), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        await _identityProvider.DidNotReceiveWithAnyArgs()
            .CreateIdentityAsync(default!, default!, default);
        await _repository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Execute_DeveDesfazerIdentidadeNoProvedor_QuandoGravacaoNaPlataformaFalha()
    {
        _identityProvider.CreateIdentityAsync("Maria Silva", "maria.silva@corretora.com.br", Arg.Any<CancellationToken>())
            .Returns("casdoor-id-123");
        _unitOfWork.CommitAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("falha de persistência"));

        var act = () => _useCase.ExecuteAsync(Request(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await _identityProvider.Received(1)
            .RemoveIdentityAsync("casdoor-id-123", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveNormalizarEmail_QuandoInformadoComMaiusculasEEspacos()
    {
        _identityProvider.CreateIdentityAsync(Arg.Any<string>(), "maria.silva@corretora.com.br", Arg.Any<CancellationToken>())
            .Returns("casdoor-id-123");

        var response = await _useCase.ExecuteAsync(
            new CreateUserRequest("Maria Silva", "  Maria.Silva@Corretora.com.br "),
            CancellationToken.None);

        response.Email.Should().Be("maria.silva@corretora.com.br");
    }
}
