using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.CreateInsurer;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.CreateInsurer.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.InsurerUseCases.CreateInsurer;

/// <summary>RN-005 — Criação de Seguradora no catálogo.</summary>
[Trait("RuleId", "RN-005")]
public class CreateInsurerUseCaseTests
{
    private readonly IInsurerRepository _repository = Substitute.For<IInsurerRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateInsurerUseCase _useCase;

    public CreateInsurerUseCaseTests()
        => _useCase = new CreateInsurerUseCase(_repository, _unitOfWork);

    private static CreateInsurerRequest ValidRequest()
        => new("12.345.678/0001-95", "Seguradora Alfa S.A.", "Alfa", "https://cdn.alfa.com/logo.png", "Active");

    [Fact]
    public async Task Execute_DeveCriarSeguradora_QuandoDadosValidos()
    {
        _repository.CnpjExistsAsync("12345678000195", null, Arg.Any<CancellationToken>())
            .Returns(false);

        var response = await _useCase.ExecuteAsync(ValidRequest(), CancellationToken.None);

        response.Cnpj.Should().Be("12345678000195");
        response.Status.Should().Be("Active");
        await _repository.Received(1).AddAsync(Arg.Any<Insurer>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoCnpjJaCadastrado()
    {
        _repository.CnpjExistsAsync("12345678000195", null, Arg.Any<CancellationToken>())
            .Returns(true);

        var act = () => _useCase.ExecuteAsync(ValidRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }
}
