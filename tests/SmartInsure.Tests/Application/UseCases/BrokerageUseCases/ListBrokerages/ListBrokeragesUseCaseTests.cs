using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ListBrokerages;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ListBrokerages.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.BrokerageUseCases.ListBrokerages;

/// <summary>RN-018 — Listagem de Corretoras.</summary>
[Trait("RuleId", "RN-018")]
public class ListBrokeragesUseCaseTests
{
    private readonly IPersonRepository _repository = Substitute.For<IPersonRepository>();
    private readonly ListBrokeragesUseCase _useCase;

    public ListBrokeragesUseCaseTests()
        => _useCase = new ListBrokeragesUseCase(_repository);

    private void RepositoryReturns(EPersonRoleStatus? status)
        => _repository.ListBrokeragesAsync(1, 20, status, Arg.Any<CancellationToken>())
            .Returns(((IReadOnlyList<BrokerageListItemDto>)
                [new BrokerageListItemDto(Guid.NewGuid(), "11444777000161", "Alfa Ltda", "Alfa", true, "Active")], 1L));

    [Fact]
    public async Task Execute_DeveListarAtivasEInativas_QuandoSemFiltro()
    {
        RepositoryReturns(null);

        var response = await _useCase.ExecuteAsync(new ListBrokeragesRequest(), CancellationToken.None);

        response.Items.Should().ContainSingle();
        await _repository.Received(1).ListBrokeragesAsync(1, 20, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveFiltrarPorSituacao_QuandoStatusInformado()
    {
        RepositoryReturns(EPersonRoleStatus.Inactive);

        await _useCase.ExecuteAsync(
            new ListBrokeragesRequest { Status = "Inactive" }, CancellationToken.None);

        await _repository.Received(1).ListBrokeragesAsync(
            1, 20, EPersonRoleStatus.Inactive, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoSituacaoDesconhecida()
    {
        var action = () => _useCase.ExecuteAsync(
            new ListBrokeragesRequest { Status = "Suspensa" }, CancellationToken.None);

        await action.Should().ThrowAsync<BusinessRuleException>();
    }
}
