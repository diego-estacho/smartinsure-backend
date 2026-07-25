using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.GetBrokerageHistory;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.GetBrokerageHistory.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.BrokerageUseCases.GetBrokerageHistory;

/// <summary>RN-055 — Histórico da Corretora.</summary>
[Trait("RuleId", "RN-055")]
public class GetBrokerageHistoryUseCaseTests
{
    private readonly IPersonRepository _repository = Substitute.For<IPersonRepository>();
    private readonly GetBrokerageHistoryUseCase _useCase;

    public GetBrokerageHistoryUseCaseTests()
        => _useCase = new GetBrokerageHistoryUseCase(_repository);

    [Fact]
    public async Task Execute_DeveRetornarEventos_QuandoCorretoraExiste()
    {
        var id = Guid.NewGuid();
        _repository.GetBrokerageHistoryAsync(id, Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<BrokerageHistoryEventDto>)
            [
                new("insurer-enabled", "Junto Seguros",
                    new DateTime(2026, 7, 12, 14, 22, 0, DateTimeKind.Utc), "Marina Bertoldi"),
                new("created", null,
                    new DateTime(2026, 7, 1, 9, 0, 0, DateTimeKind.Utc), "Marina Bertoldi"),
            ]);

        var response = await _useCase.ExecuteAsync(
            new GetBrokerageHistoryRequest(id), CancellationToken.None);

        response.Events.Should().HaveCount(2);
        response.Events[0].Type.Should().Be("insurer-enabled");
        response.Events[0].Subject.Should().Be("Junto Seguros");
    }

    [Fact]
    public async Task Execute_DeveLancarNotFound_QuandoTimelineVazia()
    {
        _repository.GetBrokerageHistoryAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<BrokerageHistoryEventDto>)[]);

        var action = () => _useCase.ExecuteAsync(
            new GetBrokerageHistoryRequest(Guid.NewGuid()), CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>();
    }
}
