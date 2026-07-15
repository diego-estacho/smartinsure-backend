using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SmartInsure.Application.UseCase.UseCases.Users.CreateUser;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.Users;

/// <summary>RN-001 — Criação de Usuário.</summary>
[Trait("RuleId", "RN-001")]
public class CreateUserUseCaseTests
{
    private readonly IUserRepository _repository = Substitute.For<IUserRepository>();
    private readonly IIdentityProvider _identityProvider = Substitute.For<IIdentityProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateUserUseCase _useCase;

    public CreateUserUseCaseTests()
        => _useCase = new CreateUserUseCase(_repository, _identityProvider, _unitOfWork);

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
