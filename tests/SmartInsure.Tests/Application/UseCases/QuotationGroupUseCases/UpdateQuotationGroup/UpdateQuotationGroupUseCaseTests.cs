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

    private readonly IQuotationRepository _quotationRepository = Substitute.For<IQuotationRepository>();
    private readonly IPersonRepository _personRepository = Substitute.For<IPersonRepository>();
    private readonly IModalityRepository _modalityRepository = Substitute.For<IModalityRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateQuotationGroupUseCase _useCase;

    public UpdateQuotationGroupUseCaseTests()
        => _useCase = new UpdateQuotationGroupUseCase(
            _quotationGroupRepository, _quotationRepository, _personRepository, _modalityRepository, _unitOfWork);

    private static QuotationGroup ExistingDraft()
        => QuotationGroup.Create(
            Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(),
            500m, new DateOnly(2026, 1, 1), new DateOnly(2026, 2, 1),
            EQuotationScopeMode.All, [], false, false);

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
            group.Id, Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(),
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
            Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(),
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
            group.Id, Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(),
            1000m, new DateOnly(2026, 1, 1), new DateOnly(2026, 2, 1),
            "All", [], false, false);

        var act = () => _useCase.ExecuteAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    [Trait("RuleId", "RN-060")]
    public async Task Execute_DeveRecusar_QuandoGrupoJaTemCotacoes()
    {
        // RN-060: Grupo já cotado é imutável nos dados-base — a edição no lugar é recusada (fail-closed);
        // a mudança segue pela criação de um novo Grupo (fork no front).
        var group = ExistingDraft();
        _quotationGroupRepository.GetByIdWithInsurersAsync(group.Id, Arg.Any<CancellationToken>())
            .Returns(group);
        _quotationRepository.ExistsForGroupAsync(group.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        var request = new UpdateQuotationGroupRequest(
            group.Id, Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(),
            1000m, new DateOnly(2026, 1, 1), new DateOnly(2026, 2, 1),
            "All", [], false, false);

        var act = () => _useCase.ExecuteAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }
}
