using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.ChangeInsurerStatus;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.ChangeInsurerStatus.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.InsurerUseCases.ChangeInsurerStatus;

/// <summary>RN-009 — Ativação e desativação de Seguradora.</summary>
[Trait("RuleId", "RN-009")]
public class ChangeInsurerStatusUseCaseTests
{
    private readonly IInsurerRepository _repository = Substitute.For<IInsurerRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ChangeInsurerStatusUseCase _useCase;

    public ChangeInsurerStatusUseCaseTests()
        => _useCase = new ChangeInsurerStatusUseCase(_repository, _unitOfWork);

    private Insurer ExistingInsurer(EInsurerStatus status)
    {
        var insurer = Insurer.Create(
            "12345678000195", "Seguradora Alfa S.A.", null, null, status);
        _repository.GetByIdAsync(insurer.Id, Arg.Any<CancellationToken>()).Returns(insurer);
        return insurer;
    }

    [Fact]
    public async Task Execute_DeveDesativar_QuandoSeguradoraAtiva()
    {
        var insurer = ExistingInsurer(EInsurerStatus.Active);

        var response = await _useCase.ExecuteAsync(
            new ChangeInsurerStatusRequest(insurer.Id, "Inactive"), CancellationToken.None);

        response.Status.Should().Be("Inactive");
        insurer.Status.Should().Be(EInsurerStatus.Inactive);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveAtivar_QuandoSeguradoraInativa()
    {
        var insurer = ExistingInsurer(EInsurerStatus.Inactive);

        var response = await _useCase.ExecuteAsync(
            new ChangeInsurerStatusRequest(insurer.Id, "Active"), CancellationToken.None);

        response.Status.Should().Be("Active");
        insurer.Status.Should().Be(EInsurerStatus.Active);
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoSeguradoraJaEstaNaSituacaoPedida()
    {
        var insurer = ExistingInsurer(EInsurerStatus.Active);

        var act = () => _useCase.ExecuteAsync(
            new ChangeInsurerStatusRequest(insurer.Id, "Active"), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    public async Task Execute_DeveLancarNotFound_QuandoSeguradoraInexistente()
    {
        var act = () => _useCase.ExecuteAsync(
            new ChangeInsurerStatusRequest(Guid.NewGuid(), "Active"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoSituacaoDesconhecida()
    {
        var insurer = ExistingInsurer(EInsurerStatus.Active);

        var act = () => _useCase.ExecuteAsync(
            new ChangeInsurerStatusRequest(insurer.Id, "Suspensa"), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }
}
