using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ChangeModalityStatus;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ChangeModalityStatus.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.CreateModality;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.CreateModality.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.ModalityUseCases;

/// <summary>RN-029 — Criação de Modalidade: nome único e Grupo existente; criação sempre humana.</summary>
[Trait("RuleId", "RN-029")]
public class CreateModalityUseCaseTests
{
    private readonly IModalityRepository _modalityRepository = Substitute.For<IModalityRepository>();
    private readonly IModalityGroupRepository _groupRepository = Substitute.For<IModalityGroupRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateModalityUseCase _useCase;

    public CreateModalityUseCaseTests()
        => _useCase = new CreateModalityUseCase(_modalityRepository, _groupRepository, _unitOfWork);

    private static ModalityGroup ExistingGroup()
        => ModalityGroup.Create("Garantias de Contrato", null, 1, EModalityGroupStatus.Active);

    [Fact]
    public async Task Execute_DeveCriarModalidade_QuandoNomeIneditoEGrupoExiste()
    {
        var group = ExistingGroup();
        _groupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>()).Returns(group);
        _modalityRepository.NameExistsAsync("Garantia de Execução de Contrato", null, Arg.Any<CancellationToken>())
            .Returns(false);

        var response = await _useCase.ExecuteAsync(
            new CreateModalityRequest("Garantia de Execução de Contrato", group.Id, "Performance", "Active"),
            CancellationToken.None);

        response.Name.Should().Be("Garantia de Execução de Contrato");
        response.ModalityGroupId.Should().Be(group.Id);
        response.Status.Should().Be("Active");
        await _modalityRepository.Received(1).AddAsync(Arg.Any<Modality>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoNomeJaExiste()
    {
        var group = ExistingGroup();
        _groupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>()).Returns(group);
        _modalityRepository.NameExistsAsync("Garantia de Execução de Contrato", null, Arg.Any<CancellationToken>())
            .Returns(true);

        var act = () => _useCase.ExecuteAsync(
            new CreateModalityRequest("Garantia de Execução de Contrato", group.Id, null, "Active"),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoGrupoNaoExiste()
    {
        var groupId = Guid.CreateVersion7();
        _groupRepository.GetByIdAsync(groupId, Arg.Any<CancellationToken>()).Returns((ModalityGroup?)null);
        _modalityRepository.NameExistsAsync(Arg.Any<string>(), null, Arg.Any<CancellationToken>()).Returns(false);

        var act = () => _useCase.ExecuteAsync(
            new CreateModalityRequest("Garantia Judicial", groupId, null, "Active"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _modalityRepository.DidNotReceive().AddAsync(Arg.Any<Modality>(), Arg.Any<CancellationToken>());
    }
}

/// <summary>RN-036 — Transição de situação da Modalidade; mesma situação é conflito.</summary>
[Trait("RuleId", "RN-036")]
public class ChangeModalityStatusUseCaseTests
{
    private readonly IModalityRepository _repository = Substitute.For<IModalityRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ChangeModalityStatusUseCase _useCase;

    public ChangeModalityStatusUseCaseTests() => _useCase = new ChangeModalityStatusUseCase(_repository, _unitOfWork);

    [Fact]
    public async Task Execute_DeveRecusar_QuandoJaNaSituacaoAlvo()
    {
        var modality = Modality.Create("Garantia Judicial", Guid.CreateVersion7(), null, EModalityStatus.Inactive);
        _repository.GetByIdAsync(modality.Id, Arg.Any<CancellationToken>()).Returns(modality);

        var act = () => _useCase.ExecuteAsync(
            new ChangeModalityStatusRequest(modality.Id, "Inactive"), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Execute_DeveReativar_QuandoInativa()
    {
        var modality = Modality.Create("Garantia Judicial", Guid.CreateVersion7(), null, EModalityStatus.Inactive);
        _repository.GetByIdAsync(modality.Id, Arg.Any<CancellationToken>()).Returns(modality);

        var response = await _useCase.ExecuteAsync(
            new ChangeModalityStatusRequest(modality.Id, "Active"), CancellationToken.None);

        response.Status.Should().Be("Active");
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}
