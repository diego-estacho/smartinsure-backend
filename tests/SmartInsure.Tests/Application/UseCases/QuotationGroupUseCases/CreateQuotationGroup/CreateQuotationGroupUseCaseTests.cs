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
        string scopeMode = "All", IReadOnlyList<Guid>? insurerIds = null)
        => new(
            PolicyHolderId, InsuredId, ModalityId,
            1000m, new DateOnly(2026, 1, 1), new DateOnly(2026, 2, 1),
            scopeMode, insurerIds ?? [], false, false);

    private void SetupValidReferences()
    {
        _personRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Person.Create("11444777000161", "Alfa Ltda", null, Guid.NewGuid()));
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
        // Sem setup do personRepository: GetByIdAsync devolve null e o tomador (checado primeiro) falta.
        var act = () => _useCase.ExecuteAsync(ValidRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoModalidadeNaoEncontrada()
    {
        _personRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Person.Create("11444777000161", "Alfa Ltda", null, Guid.NewGuid()));
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
}
