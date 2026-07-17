using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ChangeBrokerageStatus;
using SmartInsure.Application.UseCase.UseCases.BrokerageUseCases.ChangeBrokerageStatus.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.BrokerageUseCases.ChangeBrokerageStatus;

/// <summary>RN-021 — Ativação e inativação de Corretora.</summary>
[Trait("RuleId", "RN-021")]
public class ChangeBrokerageStatusUseCaseTests
{
    private readonly IPersonRepository _repository = Substitute.For<IPersonRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ChangeBrokerageStatusUseCase _useCase;

    public ChangeBrokerageStatusUseCaseTests()
        => _useCase = new ChangeBrokerageStatusUseCase(_repository, _unitOfWork);

    private Person Brokerage(EPersonRoleStatus status)
    {
        var person = Person.Create("11444777000161", "Alfa Ltda", null, Guid.NewGuid());
        person.AssignRole(EPersonRole.Broker);
        var role = person.GetRole(EPersonRole.Broker)!;
        if (status == EPersonRoleStatus.Inactive)
        {
            role.Deactivate();
        }

        _repository.GetTrackedBrokerageByIdAsync(person.Id, Arg.Any<CancellationToken>())
            .Returns(person);
        return person;
    }

    [Fact]
    public async Task Execute_DeveInativar_QuandoCorretoraAtiva()
    {
        var person = Brokerage(EPersonRoleStatus.Active);

        var response = await _useCase.ExecuteAsync(
            new ChangeBrokerageStatusRequest(person.Id, "Inactive"), CancellationToken.None);

        response.Status.Should().Be("Inactive");
        person.GetRole(EPersonRole.Broker)!.Status.Should().Be(EPersonRoleStatus.Inactive);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveAtivar_QuandoCorretoraInativa()
    {
        var person = Brokerage(EPersonRoleStatus.Inactive);

        var response = await _useCase.ExecuteAsync(
            new ChangeBrokerageStatusRequest(person.Id, "Active"), CancellationToken.None);

        response.Status.Should().Be("Active");
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoCorretoraJaEstaNaSituacaoPedida()
    {
        var person = Brokerage(EPersonRoleStatus.Active);

        var action = () => _useCase.ExecuteAsync(
            new ChangeBrokerageStatusRequest(person.Id, "Active"), CancellationToken.None);

        await action.Should().ThrowAsync<ConflictException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    public async Task Execute_DeveLancarNotFound_QuandoCorretoraInexistente()
    {
        var action = () => _useCase.ExecuteAsync(
            new ChangeBrokerageStatusRequest(Guid.NewGuid(), "Active"), CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoSituacaoDesconhecida()
    {
        var person = Brokerage(EPersonRoleStatus.Active);

        var action = () => _useCase.ExecuteAsync(
            new ChangeBrokerageStatusRequest(person.Id, "Suspensa"), CancellationToken.None);

        await action.Should().ThrowAsync<BusinessRuleException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }
}
