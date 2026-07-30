using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.UpdateQuotationGroup;
using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.UpdateQuotationGroup.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.QuotationGroupUseCases.UpdateQuotationGroup;

/// <summary>RN-051 — atualização do Grupo de Cotação em Rascunho (no lugar, mesmo id).</summary>
[Trait("RuleId", "RN-051")]
public class UpdateQuotationGroupUseCaseTests
{
    private readonly IQuotationGroupRepository _quotationGroupRepository =
        Substitute.For<IQuotationGroupRepository>();

    private readonly IPersonRepository _personRepository = Substitute.For<IPersonRepository>();
    private readonly IModalityRepository _modalityRepository = Substitute.For<IModalityRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateQuotationGroupUseCase _useCase;

    public UpdateQuotationGroupUseCaseTests()
        => _useCase = new UpdateQuotationGroupUseCase(
            _quotationGroupRepository, _personRepository, _modalityRepository, _unitOfWork);

    private static QuotationGroup ExistingDraft(Guid? branchPersonId = null)
        => QuotationGroup.Create(
            Guid.CreateVersion7(), branchPersonId, Guid.CreateVersion7(), Guid.CreateVersion7(),
            500m, new DateOnly(2026, 1, 1), new DateOnly(2026, 2, 1),
            EQuotationScopeMode.All, [], false, false);

    private static UpdateQuotationGroupRequest ValidRequest(
        Guid groupId, Guid policyHolderId, Guid? branchId = null)
        => new(
            groupId, policyHolderId, branchId, Guid.CreateVersion7(), Guid.CreateVersion7(),
            1000m, new DateOnly(2026, 1, 1), new DateOnly(2026, 2, 1),
            "All", [], false, false);

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
        // RN-051: Tomador precisa ter o papel PolicyHolder e Segurado o papel Insured. A mesma pessoa
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
    public async Task Execute_DeveAtualizarNoLugarMantendoRascunho_QuandoGrupoExiste()
    {
        var group = ExistingDraft();
        _quotationGroupRepository.GetByIdWithInsurersAsync(group.Id, Arg.Any<CancellationToken>())
            .Returns(group);
        SetupValidReferences();

        var insurer = Guid.CreateVersion7();
        var request = new UpdateQuotationGroupRequest(
            group.Id, Guid.CreateVersion7(), null, Guid.CreateVersion7(), Guid.CreateVersion7(),
            2000m, new DateOnly(2026, 3, 1), new DateOnly(2026, 6, 1),
            "Specific", [insurer], true, true);

        var response = await _useCase.ExecuteAsync(request, CancellationToken.None);

        response.Id.Should().Be(group.Id);
        response.Status.Should().Be("Draft");
        response.InsuredAmount.Should().Be(2000m);
        response.ScopeMode.Should().Be("Specific");
        response.InsurerIds.Should().BeEquivalentTo([insurer]);
        response.IncludesPenaltyCoverage.Should().BeTrue();
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoGrupoNaoEncontrado()
    {
        _quotationGroupRepository.GetByIdWithInsurersAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((QuotationGroup?)null);

        var request = new UpdateQuotationGroupRequest(
            Guid.CreateVersion7(), Guid.CreateVersion7(), null, Guid.CreateVersion7(), Guid.CreateVersion7(),
            1000m, new DateOnly(2026, 1, 1), new DateOnly(2026, 2, 1),
            "All", [], false, false);

        var act = () => _useCase.ExecuteAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoTomadorNaoEncontrado()
    {
        var group = ExistingDraft();
        _quotationGroupRepository.GetByIdWithInsurersAsync(group.Id, Arg.Any<CancellationToken>())
            .Returns(group);
        // personRepository sem setup → GetByIdWithRolesAsync devolve null → tomador não encontrado.

        var request = new UpdateQuotationGroupRequest(
            group.Id, Guid.CreateVersion7(), null, Guid.CreateVersion7(), Guid.CreateVersion7(),
            1000m, new DateOnly(2026, 1, 1), new DateOnly(2026, 2, 1),
            "All", [], false, false);

        var act = () => _useCase.ExecuteAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    [Trait("RuleId", "RN-102")]
    public async Task ExecuteAsync_ComFilialDoProprioTomador_DevePersistirOEstabelecimento()
    {
        var headquarters = CreateHeadquarters();
        var branch = CreateBranchOf(headquarters, "11222333000262");
        var group = ExistingDraft();
        _quotationGroupRepository.GetByIdWithInsurersAsync(group.Id, Arg.Any<CancellationToken>())
            .Returns(group);
        _personRepository.GetTrackedByIdAsync(branch.Id, Arg.Any<CancellationToken>()).Returns(branch);
        SetupValidReferences();

        var request = ValidRequest(group.Id, headquarters.Id, branchId: branch.Id);

        await _useCase.ExecuteAsync(request, CancellationToken.None);

        group.BranchPersonId.Should().Be(branch.Id);
    }

    [Fact]
    [Trait("RuleId", "RN-102")]
    public async Task ExecuteAsync_ComFilialDeOutraMatriz_DeveRecusar()
    {
        var headquarters = CreateHeadquarters();
        var otherHeadquarters = CreateHeadquarters("99888777000181", "Matriz Alheia LTDA");
        var branchOfOther = CreateBranchOf(otherHeadquarters, "99888777000262");
        var group = ExistingDraft();
        _quotationGroupRepository.GetByIdWithInsurersAsync(group.Id, Arg.Any<CancellationToken>())
            .Returns(group);
        _personRepository.GetTrackedByIdAsync(branchOfOther.Id, Arg.Any<CancellationToken>()).Returns(branchOfOther);
        SetupValidReferences();

        var request = ValidRequest(group.Id, headquarters.Id, branchId: branchOfOther.Id);
        var act = () => _useCase.ExecuteAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    [Trait("RuleId", "RN-102")]
    public async Task ExecuteAsync_ComFilialInexistente_DeveRecusar()
    {
        var group = ExistingDraft();
        _quotationGroupRepository.GetByIdWithInsurersAsync(group.Id, Arg.Any<CancellationToken>())
            .Returns(group);
        var inexistentBranchId = Guid.CreateVersion7();
        _personRepository.GetTrackedByIdAsync(inexistentBranchId, Arg.Any<CancellationToken>())
            .Returns((Person?)null);
        SetupValidReferences();

        var request = ValidRequest(group.Id, Guid.CreateVersion7(), branchId: inexistentBranchId);
        var act = () => _useCase.ExecuteAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    [Trait("RuleId", "RN-102")]
    public async Task ExecuteAsync_SemFilial_DeveManterEstabelecimentoNulo()
    {
        var group = ExistingDraft();
        _quotationGroupRepository.GetByIdWithInsurersAsync(group.Id, Arg.Any<CancellationToken>())
            .Returns(group);
        SetupValidReferences();

        var request = ValidRequest(group.Id, Guid.CreateVersion7(), branchId: null);

        await _useCase.ExecuteAsync(request, CancellationToken.None);

        group.BranchPersonId.Should().BeNull();
        await _personRepository.DidNotReceive().GetTrackedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-102")]
    public async Task ExecuteAsync_ComGrupoTinhaFilialEUpdateChegaSemBranchId_DeveLimparOEstabelecimento()
    {
        // RN-102: trocar o Tomador limpa a Filial — sem branchId na atualização, o estabelecimento
        // que já existia no grupo (Filial de uma cotação anterior) some, mesmo sem revalidar contra o Tomador.
        var group = ExistingDraft(branchPersonId: Guid.CreateVersion7());
        _quotationGroupRepository.GetByIdWithInsurersAsync(group.Id, Arg.Any<CancellationToken>())
            .Returns(group);
        SetupValidReferences();

        var request = ValidRequest(group.Id, Guid.CreateVersion7(), branchId: null);

        await _useCase.ExecuteAsync(request, CancellationToken.None);

        group.BranchPersonId.Should().BeNull();
    }
}
