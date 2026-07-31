using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.CreateQuotationGroup;
using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.CreateQuotationGroup.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.QuotationGroupUseCases.CreateQuotationGroup;

/// <summary>RN-050 — criação do Grupo de Cotação em Rascunho ao concluir a etapa de risco.</summary>
[Trait("RuleId", "RN-050")]
public class CreateQuotationGroupUseCaseTests
{
    private static readonly Guid PolicyHolderId = Guid.CreateVersion7();
    private static readonly Guid InsuredId = Guid.CreateVersion7();
    private static readonly Guid ModalityId = Guid.CreateVersion7();

    private readonly IQuotationGroupRepository _quotationGroupRepository =
        Substitute.For<IQuotationGroupRepository>();

    private readonly IPersonRepository _personRepository = Substitute.For<IPersonRepository>();
    private readonly IModalityRepository _modalityRepository = Substitute.For<IModalityRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateQuotationGroupUseCase _useCase;

    public CreateQuotationGroupUseCaseTests()
        => _useCase = new CreateQuotationGroupUseCase(
            _quotationGroupRepository, _personRepository, _modalityRepository, _unitOfWork);

    private static CreateQuotationGroupRequest ValidRequest(
        string scopeMode = "All", IReadOnlyList<Guid>? insurerIds = null, Guid? branchId = null, Guid? policyHolderId = null)
        => new(
            policyHolderId ?? PolicyHolderId, branchId, InsuredId, ModalityId,
            1000m, new DateOnly(2026, 1, 1), new DateOnly(2026, 2, 1),
            scopeMode, insurerIds ?? [], false, false);

    /// <summary>RN-101: matriz sintética (CNPJ de ordem /0001) para os cenários de RN-102.</summary>
    private static Person CreateHeadquarters(string documentNumber = "11222333000181", string name = "Matriz LTDA")
        => Person.Create(documentNumber, name, null, Guid.NewGuid());

    /// <summary>RN-101: filial sintética já vinculada à matriz informada.</summary>
    private static Person CreateBranchOf(Person headquarters, string documentNumber, string name = "Filial LTDA")
    {
        var branch = Person.Create(documentNumber, name, null, Guid.NewGuid());
        branch.LinkToHeadquarters(headquarters);
        return branch;
    }

    private void SetupValidReferences()
    {
        // RN-050: Tomador precisa ter o papel PolicyHolder e Segurado o papel Insured. A mesma pessoa
        // acumula os dois papéis (RN-017), então serve para as duas checagens do caso de uso.
        var person = Person.Create("11444777000161", "Alfa Ltda", null, Guid.NewGuid());
        person.AssignRole(EPersonRole.PolicyHolder);
        person.AssignRole(EPersonRole.Insured);
        _personRepository.GetByIdWithRolesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(person);
        _modalityRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Modality.CreateManual("Garantia de Execução", null, EModalityStatus.Active));
    }

    [Fact]
    public async Task Execute_DeveCriarGrupoEmRascunho_QuandoDadosValidos()
    {
        SetupValidReferences();

        var response = await _useCase.ExecuteAsync(ValidRequest(), CancellationToken.None);

        response.Status.Should().Be("Draft");
        response.ScopeMode.Should().Be("All");
        response.InsurerIds.Should().BeEmpty();
        response.PolicyHolderId.Should().Be(PolicyHolderId);
        response.InsuredId.Should().Be(InsuredId);
        response.ModalityId.Should().Be(ModalityId);
        await _quotationGroupRepository.Received(1)
            .AddAsync(Arg.Any<QuotationGroup>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveGuardarSeguradorasDoEscopo_QuandoScopeSpecific()
    {
        SetupValidReferences();
        var insurerA = Guid.CreateVersion7();
        var insurerB = Guid.CreateVersion7();

        var response = await _useCase.ExecuteAsync(
            ValidRequest("Specific", [insurerA, insurerB]), CancellationToken.None);

        response.ScopeMode.Should().Be("Specific");
        response.InsurerIds.Should().BeEquivalentTo([insurerA, insurerB]);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoTomadorNaoEncontrado()
    {
        // Sem setup do personRepository: GetByIdWithRolesAsync devolve null e o tomador (checado primeiro) falta.
        var act = () => _useCase.ExecuteAsync(ValidRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoModalidadeNaoEncontrada()
    {
        var person = Person.Create("11444777000161", "Alfa Ltda", null, Guid.NewGuid());
        person.AssignRole(EPersonRole.PolicyHolder);
        person.AssignRole(EPersonRole.Insured);
        _personRepository.GetByIdWithRolesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(person);
        // modalityRepository sem setup → GetByIdAsync devolve null → modalidade não encontrada.

        var act = () => _useCase.ExecuteAsync(ValidRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoEscopoInvalido()
    {
        var act = () => _useCase.ExecuteAsync(ValidRequest("Xpto"), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    [Trait("RuleId", "RN-102")]
    public async Task ExecuteAsync_ComFilialDoProprioTomador_DevePersistirOEstabelecimento()
    {
        SetupValidReferences();
        var headquarters = CreateHeadquarters();
        var branch = CreateBranchOf(headquarters, "11222333000262");
        _personRepository.GetTrackedByIdAsync(branch.Id, Arg.Any<CancellationToken>()).Returns(branch);

        QuotationGroup? captured = null;
        await _quotationGroupRepository.AddAsync(Arg.Do<QuotationGroup>(g => captured = g), Arg.Any<CancellationToken>());

        await _useCase.ExecuteAsync(
            ValidRequest(branchId: branch.Id, policyHolderId: headquarters.Id), CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.BranchPersonId.Should().Be(branch.Id);
    }

    [Fact]
    [Trait("RuleId", "RN-102")]
    public async Task ExecuteAsync_ComFilialDeOutraMatriz_DeveRecusar()
    {
        SetupValidReferences();
        var headquarters = CreateHeadquarters();
        var otherHeadquarters = CreateHeadquarters("99888777000181", "Matriz Alheia LTDA");
        var branchOfOther = CreateBranchOf(otherHeadquarters, "99888777000262");
        _personRepository.GetTrackedByIdAsync(branchOfOther.Id, Arg.Any<CancellationToken>()).Returns(branchOfOther);

        var act = () => _useCase.ExecuteAsync(
            ValidRequest(branchId: branchOfOther.Id, policyHolderId: headquarters.Id), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    [Trait("RuleId", "RN-102")]
    public async Task ExecuteAsync_ComFilialInexistente_DeveRecusar()
    {
        SetupValidReferences();
        var inexistentBranchId = Guid.CreateVersion7();
        _personRepository.GetTrackedByIdAsync(inexistentBranchId, Arg.Any<CancellationToken>())
            .Returns((Person?)null);

        var act = () => _useCase.ExecuteAsync(ValidRequest(branchId: inexistentBranchId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    [Trait("RuleId", "RN-102")]
    public async Task ExecuteAsync_SemFilial_DeveManterEstabelecimentoNulo()
    {
        SetupValidReferences();

        QuotationGroup? captured = null;
        await _quotationGroupRepository.AddAsync(Arg.Do<QuotationGroup>(g => captured = g), Arg.Any<CancellationToken>());

        await _useCase.ExecuteAsync(ValidRequest(branchId: null), CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.BranchPersonId.Should().BeNull();
        await _personRepository.DidNotReceive().GetTrackedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
