using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.GetQuotationGroup;
using SmartInsure.Application.UseCase.UseCases.QuotationGroupUseCases.GetQuotationGroup.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.QuotationGroupUseCases.GetQuotationGroup;

/// <summary>RN-050/RN-051 — leitura do Grupo de Cotação para reidratar o wizard (refresh com o id na rota).</summary>
[Trait("RuleId", "RN-051")]
public class GetQuotationGroupUseCaseTests
{
    private readonly IQuotationGroupRepository _quotationGroupRepository =
        Substitute.For<IQuotationGroupRepository>();

    private readonly IPersonRepository _personRepository = Substitute.For<IPersonRepository>();
    private readonly IModalityRepository _modalityRepository = Substitute.For<IModalityRepository>();
    private readonly GetQuotationGroupUseCase _useCase;

    public GetQuotationGroupUseCaseTests()
        => _useCase = new GetQuotationGroupUseCase(
            _quotationGroupRepository, _personRepository, _modalityRepository);

    private static PersonSearchItemDto PersonSummary(Guid id, string name, string? social = null)
        => new(
            id, "11444777000161", name, social, "J", true, ["PolicyHolder"],
            new PersonMainAddressDto("01310-100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP"),
            []);

    [Fact]
    public async Task Execute_DeveResolverTomadorSeguradoEModalidade_QuandoGrupoExiste()
    {
        var policyHolderId = Guid.CreateVersion7();
        var insuredId = Guid.CreateVersion7();
        var modalityId = Guid.CreateVersion7();
        var insurer = Guid.CreateVersion7();
        var multa = Guid.CreateVersion7();

        var group = QuotationGroup.Create(
            policyHolderId, branchPersonId: null, insuredId, modalityId,
            2000m, new DateOnly(2026, 3, 1), new DateOnly(2026, 6, 1),
            EQuotationScopeMode.Specific, [insurer], [multa]);

        _quotationGroupRepository.GetByIdWithInsurersAsync(group.Id, Arg.Any<CancellationToken>())
            .Returns(group);
        _personRepository.GetSummaryByIdAsync(policyHolderId, Arg.Any<CancellationToken>())
            .Returns(PersonSummary(policyHolderId, "Tomador Ltda"));
        _personRepository.GetSummaryByIdAsync(insuredId, Arg.Any<CancellationToken>())
            .Returns(PersonSummary(insuredId, "Segurado SA", "Segurado"));
        _modalityRepository.GetByIdAsync(modalityId, Arg.Any<CancellationToken>())
            .Returns(Modality.CreateManual("Garantia de Execução", null, EModalityStatus.Active));

        var response = await _useCase.ExecuteAsync(
            new GetQuotationGroupRequest(group.Id), CancellationToken.None);

        response.Id.Should().Be(group.Id);
        response.ModalityId.Should().Be(modalityId);
        response.ModalityName.Should().Be("Garantia de Execução");
        response.InsuredAmount.Should().Be(2000m);
        response.ScopeMode.Should().Be("Specific");
        response.InsurerIds.Should().BeEquivalentTo([insurer]);
        // RN-104: o wizard reidrata as Coberturas Adicionais escolhidas pelos ids da canônica.
        response.AdditionalCoverageIds.Should().BeEquivalentTo(new[] { multa });
        response.Status.Should().Be("Draft");
        response.PolicyHolder.Id.Should().Be(policyHolderId);
        response.PolicyHolder.Name.Should().Be("Tomador Ltda");
        response.PolicyHolder.MainAddress!.City.Should().Be("São Paulo");
        response.Insured.Id.Should().Be(insuredId);
        response.Insured.SocialName.Should().Be("Segurado");
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoGrupoNaoEncontrado()
    {
        _quotationGroupRepository.GetByIdWithInsurersAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((QuotationGroup?)null);

        var act = () => _useCase.ExecuteAsync(
            new GetQuotationGroupRequest(Guid.CreateVersion7()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoTomadorNaoEncontrado()
    {
        var group = QuotationGroup.Create(
            Guid.CreateVersion7(), branchPersonId: null, Guid.CreateVersion7(), Guid.CreateVersion7(),
            500m, new DateOnly(2026, 1, 1), new DateOnly(2026, 2, 1),
            EQuotationScopeMode.All, [], []);
        _quotationGroupRepository.GetByIdWithInsurersAsync(group.Id, Arg.Any<CancellationToken>())
            .Returns(group);
        // _personRepository sem setup → GetSummaryByIdAsync devolve null → tomador não encontrado.

        var act = () => _useCase.ExecuteAsync(
            new GetQuotationGroupRequest(group.Id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
