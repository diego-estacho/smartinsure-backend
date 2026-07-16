using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.UpdateInsurer;
using SmartInsure.Application.UseCase.UseCases.InsurerUseCases.UpdateInsurer.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.InsurerUseCases.UpdateInsurer;

/// <summary>RN-008 — Alteração de dados cadastrais da Seguradora.</summary>
[Trait("RuleId", "RN-008")]
public class UpdateInsurerUseCaseTests
{
    private readonly IInsurerRepository _repository = Substitute.For<IInsurerRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateInsurerUseCase _useCase;

    public UpdateInsurerUseCaseTests()
        => _useCase = new UpdateInsurerUseCase(_repository, _unitOfWork);

    private Insurer ExistingInsurer()
    {
        var insurer = Insurer.Create(
            "12345678000195", "Seguradora Alfa S.A.", "Alfa", null, EInsurerStatus.Inactive);
        _repository.GetByIdAsync(insurer.Id, Arg.Any<CancellationToken>()).Returns(insurer);
        return insurer;
    }

    [Fact]
    public async Task Execute_DeveAlterarDados_QuandoDadosValidos()
    {
        var insurer = ExistingInsurer();
        _repository.CnpjExistsAsync("98765432000198", insurer.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        var response = await _useCase.ExecuteAsync(
            new UpdateInsurerRequest(insurer.Id, "98.765.432/0001-98", "Seguradora Beta S.A.", null, null),
            CancellationToken.None);

        response.Cnpj.Should().Be("98765432000198");
        response.CorporateName.Should().Be("Seguradora Beta S.A.");
        response.TradeName.Should().BeNull();
        insurer.Status.Should().Be(EInsurerStatus.Inactive, "situação não muda por alteração cadastral");
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoCnpjJaUsadoPorOutraSeguradora()
    {
        var insurer = ExistingInsurer();
        _repository.CnpjExistsAsync("98765432000198", insurer.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        var act = () => _useCase.ExecuteAsync(
            new UpdateInsurerRequest(insurer.Id, "98765432000198", "Seguradora Beta S.A.", null, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    public async Task Execute_DeveLancarNotFound_QuandoSeguradoraInexistente()
    {
        var act = () => _useCase.ExecuteAsync(
            new UpdateInsurerRequest(Guid.NewGuid(), "12345678000195", "Seguradora Alfa S.A.", null, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
