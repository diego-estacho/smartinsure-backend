using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ListBrokerages;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ListBrokerages.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.BrokerageUseCases.ListBrokerages;

/// <summary>RN-018 — Listagem de Corretoras (busca, filtros combinados e contagem por situação, server-side).</summary>
[Trait("RuleId", "RN-018")]
public class ListBrokeragesUseCaseTests
{
    private readonly IPersonRepository _repository = Substitute.For<IPersonRepository>();
    private readonly ListBrokeragesUseCase _useCase;

    public ListBrokeragesUseCaseTests()
    {
        _useCase = new ListBrokeragesUseCase(_repository);

        _repository.ListBrokeragesAsync(Arg.Any<BrokerageListQuery>(), Arg.Any<CancellationToken>())
            .Returns(new BrokerageListResult(
                [new BrokerageListItemDto(
                    Guid.NewGuid(), "11444777000161", "Alfa Ltda", "Alfa", true,
                    "Active", "Active", new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    2, ["Junto Seguros", "Pottencial"], ["PlugV2"])],
                1L,
                new BrokerageSituationCountsDto(3, 1, 1, 1)));
    }

    [Fact]
    public async Task Execute_DeveListarComContagemPorSituacao_QuandoSemFiltro()
    {
        var response = await _useCase.ExecuteAsync(new ListBrokeragesRequest(), CancellationToken.None);

        response.Items.Should().ContainSingle();
        response.Counts.All.Should().Be(3);
        response.Counts.Incomplete.Should().Be(1);
        await _repository.Received(1).ListBrokeragesAsync(
            Arg.Is<BrokerageListQuery>(query => query.Situation == null && query.Search == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveMapearBuscaEFiltros_ParaConsultaServerSide()
    {
        await _useCase.ExecuteAsync(
            new ListBrokeragesRequest
            {
                Search = "alfa",
                Situation = "Incomplete",
                Sector = "Private",
                CalculationEngine = "PlugV2",
            },
            CancellationToken.None);

        await _repository.Received(1).ListBrokeragesAsync(
            Arg.Is<BrokerageListQuery>(query =>
                query.Search == "alfa"
                && query.Situation == EBrokerageSituation.Incomplete
                && query.IsPrivateSector == true
                && query.CalculationEngine == ECalculationEngine.PlugV2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoSituacaoDesconhecida()
    {
        var action = () => _useCase.ExecuteAsync(
            new ListBrokeragesRequest { Situation = "Suspensa" }, CancellationToken.None);

        await action.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoSetorInvalido()
    {
        var action = () => _useCase.ExecuteAsync(
            new ListBrokeragesRequest { Sector = "Misto" }, CancellationToken.None);

        await action.Should().ThrowAsync<BusinessRuleException>();
    }
}
