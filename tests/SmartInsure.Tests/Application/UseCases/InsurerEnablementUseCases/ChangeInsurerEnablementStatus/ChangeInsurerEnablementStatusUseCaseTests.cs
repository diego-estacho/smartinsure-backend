using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.ChangeInsurerEnablementStatus;
using SmartInsure.Application.UseCase.UseCases.InsurerEnablementUseCases.ChangeInsurerEnablementStatus.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.InsurerEnablementUseCases.ChangeInsurerEnablementStatus;

/// <summary>RN-022 — alternância Ativa ↔ Inativa da Habilitação (nunca excluída).</summary>
[Trait("RuleId", "RN-022")]
public class ChangeInsurerEnablementStatusUseCaseTests
{
    private readonly IInsurerEnablementRepository _repository =
        Substitute.For<IInsurerEnablementRepository>();

    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ChangeInsurerEnablementStatusUseCase _useCase;

    public ChangeInsurerEnablementStatusUseCaseTests()
        => _useCase = new ChangeInsurerEnablementStatusUseCase(_repository, _unitOfWork);

    private static InsurerEnablement ActiveEnablement()
        => InsurerEnablement.Create(
            Guid.CreateVersion7(), Guid.CreateVersion7(), ECalculationEngine.PlugV2, null);

    [Fact]
    public async Task Execute_DeveInativarHabilitacao_QuandoAtiva()
    {
        var enablement = ActiveEnablement();
        _repository.GetByIdAsync(enablement.Id, Arg.Any<CancellationToken>()).Returns(enablement);

        var response = await _useCase.ExecuteAsync(
            new ChangeInsurerEnablementStatusRequest(enablement.Id, "Inactive"), CancellationToken.None);

        response.Status.Should().Be("Inactive");
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoAtivarHabilitacaoJaAtiva()
    {
        var enablement = ActiveEnablement();
        _repository.GetByIdAsync(enablement.Id, Arg.Any<CancellationToken>()).Returns(enablement);

        var act = () => _useCase.ExecuteAsync(
            new ChangeInsurerEnablementStatusRequest(enablement.Id, "Active"), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    public async Task Execute_DeveReativarHabilitacao_QuandoInativa()
    {
        var enablement = ActiveEnablement();
        enablement.Deactivate();
        _repository.GetByIdAsync(enablement.Id, Arg.Any<CancellationToken>()).Returns(enablement);

        var response = await _useCase.ExecuteAsync(
            new ChangeInsurerEnablementStatusRequest(enablement.Id, "Active"), CancellationToken.None);

        response.Status.Should().Be("Active");
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoHabilitacaoNaoEncontrada()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((InsurerEnablement?)null);

        var act = () => _useCase.ExecuteAsync(
            new ChangeInsurerEnablementStatusRequest(Guid.CreateVersion7(), "Inactive"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoSituacaoDesconhecida()
    {
        var act = () => _useCase.ExecuteAsync(
            new ChangeInsurerEnablementStatusRequest(Guid.CreateVersion7(), "Suspensa"), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }
}
