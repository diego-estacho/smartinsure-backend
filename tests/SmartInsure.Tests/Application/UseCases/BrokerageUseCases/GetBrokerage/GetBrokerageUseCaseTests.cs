using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.GetBrokerage;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.GetBrokerage.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.BrokerageUseCases.GetBrokerage;

/// <summary>RN-020 — Detalhes da Corretora.</summary>
[Trait("RuleId", "RN-020")]
public class GetBrokerageUseCaseTests
{
    private readonly IPersonRepository _repository = Substitute.For<IPersonRepository>();
    private readonly GetBrokerageUseCase _useCase;

    public GetBrokerageUseCaseTests()
        => _useCase = new GetBrokerageUseCase(_repository);

    [Fact]
    public async Task Execute_DeveRetornarDetalhes_QuandoCorretoraExiste()
    {
        var id = Guid.NewGuid();
        _repository.GetBrokerageByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(new BrokerageDetailsDto(
                id,
                "11444777000161",
                "Alfa Ltda",
                "Alfa",
                "2062",
                "Sociedade Empresária Limitada",
                true,
                "Active",
                new PersonMainAddressDto("01310100", "Avenida Paulista", "1000", null, "Bela Vista", "São Paulo", "SP")));

        var response = await _useCase.ExecuteAsync(new GetBrokerageRequest(id), CancellationToken.None);

        response.DocumentNumber.Should().Be("11444777000161");
        response.LegalNatureCode.Should().Be("2062");
        response.MainAddress!.City.Should().Be("São Paulo");
    }

    [Fact]
    public async Task Execute_DeveLancarNotFound_QuandoCorretoraInexistente()
    {
        var action = () => _useCase.ExecuteAsync(
            new GetBrokerageRequest(Guid.NewGuid()), CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>();
    }
}
