using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.GetModalityGroup;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.GetModalityGroup.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.ModalityGroupUseCases;

/// <summary>RN-029 — Detalhe de Grupo de Modalidade.</summary>
[Trait("RuleId", "RN-029")]
public class GetModalityGroupUseCaseTests
{
    private readonly IModalityGroupRepository _repository = Substitute.For<IModalityGroupRepository>();
    private readonly GetModalityGroupUseCase _useCase;

    public GetModalityGroupUseCaseTests() => _useCase = new GetModalityGroupUseCase(_repository);

    [Fact]
    public async Task Execute_DeveRetornarDetalhe_QuandoEncontrado()
    {
        var group = ModalityGroup.Create("Garantias de Contrato", "Performance", 2, EModalityGroupStatus.Active);
        _repository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>()).Returns(group);

        var response = await _useCase.ExecuteAsync(new GetModalityGroupRequest(group.Id), CancellationToken.None);

        response.Name.Should().Be("Garantias de Contrato");
        response.DisplayOrder.Should().Be(2);
        response.Status.Should().Be("Active");
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoNaoEncontrado()
    {
        var id = Guid.CreateVersion7();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((ModalityGroup?)null);

        var act = () => _useCase.ExecuteAsync(new GetModalityGroupRequest(id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
