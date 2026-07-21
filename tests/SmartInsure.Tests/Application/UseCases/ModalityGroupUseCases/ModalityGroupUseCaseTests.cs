using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.ChangeModalityGroupStatus;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.ChangeModalityGroupStatus.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.CreateModalityGroup;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.CreateModalityGroup.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.ListModalityGroups;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.ListModalityGroups.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.UpdateModalityGroup;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.UpdateModalityGroup.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.ModalityGroupUseCases;

/// <summary>RN-029 — Criação de Grupo de Modalidade: nome único no catálogo.</summary>
[Trait("RuleId", "RN-029")]
public class CreateModalityGroupUseCaseTests
{
    private readonly IModalityGroupRepository _repository = Substitute.For<IModalityGroupRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateModalityGroupUseCase _useCase;

    public CreateModalityGroupUseCaseTests() => _useCase = new CreateModalityGroupUseCase(_repository, _unitOfWork);

    private static CreateModalityGroupRequest ValidRequest()
        => new("Garantias de Licitação", "Bid bonds", 1, "Active");

    [Fact]
    public async Task Execute_DeveCriarGrupo_QuandoDadosValidos()
    {
        _repository.NameExistsAsync("Garantias de Licitação", null, Arg.Any<CancellationToken>()).Returns(false);

        var response = await _useCase.ExecuteAsync(ValidRequest(), CancellationToken.None);

        response.Name.Should().Be("Garantias de Licitação");
        response.Status.Should().Be("Active");
        await _repository.Received(1).AddAsync(Arg.Any<ModalityGroup>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoNomeJaExiste()
    {
        _repository.NameExistsAsync("Garantias de Licitação", null, Arg.Any<CancellationToken>()).Returns(true);

        var act = () => _useCase.ExecuteAsync(ValidRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }
}

/// <summary>RN-029 — Edição de Grupo de Modalidade: nome permanece único.</summary>
[Trait("RuleId", "RN-029")]
public class UpdateModalityGroupUseCaseTests
{
    private readonly IModalityGroupRepository _repository = Substitute.For<IModalityGroupRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateModalityGroupUseCase _useCase;

    public UpdateModalityGroupUseCaseTests() => _useCase = new UpdateModalityGroupUseCase(_repository, _unitOfWork);

    [Fact]
    public async Task Execute_DeveRecusar_QuandoGrupoNaoEncontrado()
    {
        var id = Guid.CreateVersion7();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((ModalityGroup?)null);

        var act = () => _useCase.ExecuteAsync(new UpdateModalityGroupRequest(id, "Novo", null, 2), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoNomeJaExisteEmOutroGrupo()
    {
        var group = ModalityGroup.Create("Garantias de Contrato", null, 1, EModalityGroupStatus.Active);
        _repository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>()).Returns(group);
        _repository.NameExistsAsync("Garantias de Licitação", group.Id, Arg.Any<CancellationToken>()).Returns(true);

        var act = () => _useCase.ExecuteAsync(
            new UpdateModalityGroupRequest(group.Id, "Garantias de Licitação", null, 1), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }
}

/// <summary>RN-036 — Transição de situação do Grupo; mesma situação é conflito.</summary>
[Trait("RuleId", "RN-036")]
public class ChangeModalityGroupStatusUseCaseTests
{
    private readonly IModalityGroupRepository _repository = Substitute.For<IModalityGroupRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ChangeModalityGroupStatusUseCase _useCase;

    public ChangeModalityGroupStatusUseCaseTests()
        => _useCase = new ChangeModalityGroupStatusUseCase(_repository, _unitOfWork);

    [Fact]
    public async Task Execute_DeveInativar_QuandoAtivo()
    {
        var group = ModalityGroup.Create("Garantias Judiciais", null, 1, EModalityGroupStatus.Active);
        _repository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>()).Returns(group);

        var response = await _useCase.ExecuteAsync(
            new ChangeModalityGroupStatusRequest(group.Id, "Inactive"), CancellationToken.None);

        response.Status.Should().Be("Inactive");
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoJaNaSituacaoAlvo()
    {
        var group = ModalityGroup.Create("Garantias Judiciais", null, 1, EModalityGroupStatus.Active);
        _repository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>()).Returns(group);

        var act = () => _useCase.ExecuteAsync(
            new ChangeModalityGroupStatusRequest(group.Id, "Active"), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoGrupoNaoEncontrado()
    {
        var id = Guid.CreateVersion7();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((ModalityGroup?)null);

        var act = () => _useCase.ExecuteAsync(new ChangeModalityGroupStatusRequest(id, "Inactive"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

/// <summary>RN-036 — Listagem do catálogo: padrão só Ativos; completa só para o Administrador.</summary>
[Trait("RuleId", "RN-036")]
public class ListModalityGroupsUseCaseTests
{
    private readonly IModalityGroupRepository _repository = Substitute.For<IModalityGroupRepository>();
    private readonly ListModalityGroupsUseCase _useCase;

    public ListModalityGroupsUseCaseTests() => _useCase = new ListModalityGroupsUseCase(_repository);

    [Fact]
    public async Task Execute_NaoDeveIncluirInativos_QuandoChamadorNaoEhAdministrador()
    {
        _repository.ListAsync(1, 20, false, Arg.Any<CancellationToken>())
            .Returns((new List<ModalityGroupListItemDto>(), 0L));

        await _useCase.ExecuteAsync(
            new ListModalityGroupsRequest { IncludeInactive = true, CallerIsSystemAdministrator = false },
            CancellationToken.None);

        await _repository.Received(1).ListAsync(1, 20, false, Arg.Any<CancellationToken>());
    }
}
