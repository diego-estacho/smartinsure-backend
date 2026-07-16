using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.GetInsurer;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.GetInsurer.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.InsurerUseCases.GetInsurer;

/// <summary>RN-010 — detalhe de Seguradora do catálogo (leitura).</summary>
[Trait("RuleId", "RN-010")]
public class GetInsurerUseCaseTests
{
    private readonly IInsurerRepository _repository = Substitute.For<IInsurerRepository>();
    private readonly GetInsurerUseCase _useCase;

    public GetInsurerUseCaseTests()
        => _useCase = new GetInsurerUseCase(_repository);

    [Fact]
    public async Task Execute_DeveRetornarSeguradora_QuandoEncontrada()
    {
        var insurerId = Guid.NewGuid();
        var insurer = Insurer.Create("12345678000195", "Seguradora Alfa S.A.", "Alfa", "https://cdn.alfa.com/logo.png", EInsurerStatus.Active);

        _repository.GetByIdAsync(insurerId, Arg.Any<CancellationToken>())
            .Returns(insurer);

        var response = await _useCase.ExecuteAsync(
            new GetInsurerRequest(insurerId),
            CancellationToken.None);

        response.Should().NotBeNull();
        response.Cnpj.Should().Be("12345678000195");
        response.CorporateName.Should().Be("Seguradora Alfa S.A.");
        response.Status.Should().Be("Active");
    }

    [Fact]
    public async Task Execute_DeveLancarNotFound_QuandoSeguradoraInexistente()
    {
        var insurerId = Guid.NewGuid();

        _repository.GetByIdAsync(insurerId, Arg.Any<CancellationToken>())
            .Returns((Insurer?)null);

        var act = () => _useCase.ExecuteAsync(
            new GetInsurerRequest(insurerId),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
