using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.LegalEntityUseCases.SearchLegalEntities;
using SmartInsure.Application.UseCase.UseCases.LegalEntityUseCases.SearchLegalEntities.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Abstractions.Services.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.LegalEntityUseCases.SearchLegalEntities;

/// <summary>
/// RN-013..RN-016 — busca de Pessoa Jurídica por nome ou documento, com importação
/// do Birô e resolução de matriz para tomador.
/// </summary>
public class SearchLegalEntitiesUseCaseTests
{
    private const string HeadquartersCnpj = "11444777000161";
    private const string BranchCnpj = "11444777000242";

    private readonly ILegalEntityRepository _legalEntityRepository =
        Substitute.For<ILegalEntityRepository>();

    private readonly ILegalNatureRepository _legalNatureRepository =
        Substitute.For<ILegalNatureRepository>();

    private readonly IBureauProvider _bureauProvider = Substitute.For<IBureauProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly SearchLegalEntitiesUseCase _useCase;

    public SearchLegalEntitiesUseCaseTests()
        => _useCase = new SearchLegalEntitiesUseCase(
            _legalEntityRepository, _legalNatureRepository, _bureauProvider, _unitOfWork);

    private static LegalEntitySearchItemDto Item(string cnpj, string name = "Alfa Ltda")
        => new(Guid.NewGuid(), cnpj, name, null, true, null);

    private static BureauPersonComplement Complement(string name = "Alfa Ltda")
        => new()
        {
            Name = name,
            TradeName = "Alfa",
            LegalNature = "206-2 - Sociedade Empresária Limitada",
            ZipCode = "01310-100",
            Street = "Avenida Paulista",
            Number = "1000",
            District = "Bela Vista",
            City = "São Paulo",
            State = "SP",
        };

    private void SearchReturns(params LegalEntitySearchItemDto[] items)
        => _legalEntityRepository.SearchByNameOrCnpjAsync(
                Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(items);

    private void BureauReturns(string cnpj, BureauPersonComplement? complement)
        => _bureauProvider.GetPersonComplementAsync(
                cnpj, Arg.Any<string>(), EBureau.ReceitaWS, Arg.Any<CancellationToken>())
            .Returns(complement);

    private void NatureExists(bool isPrivate = true)
        => _legalNatureRepository.GetByCodeAsync("2062", Arg.Any<CancellationToken>())
            .Returns(LegalNature.Create(2018, "2062", "Sociedade Empresária Limitada", isPrivate));

    [Fact]
    [Trait("RuleId", "RN-013")]
    public async Task Execute_DeveRetornarDaBase_QuandoNomeContemTermo()
    {
        SearchReturns(Item("12345678000195"), Item("98765432000181", "Alfa Corretora"));

        var response = await _useCase.ExecuteAsync(
            new SearchLegalEntitiesRequest("Alfa", "Insured"), CancellationToken.None);

        response.Items.Should().HaveCount(2);
        response.Notice.Should().BeNull();
        await _bureauProvider.DidNotReceiveWithAnyArgs()
            .GetPersonComplementAsync(default!, default!, default, default);
    }

    [Fact]
    [Trait("RuleId", "RN-013")]
    public async Task Execute_DeveRetornarSemConsultarBiro_QuandoCnpjJaCadastrado()
    {
        SearchReturns(Item(HeadquartersCnpj));

        var response = await _useCase.ExecuteAsync(
            new SearchLegalEntitiesRequest(HeadquartersCnpj, "Insured"), CancellationToken.None);

        response.Items.Should().ContainSingle(item => item.Cnpj == HeadquartersCnpj);
        await _bureauProvider.DidNotReceiveWithAnyArgs()
            .GetPersonComplementAsync(default!, default!, default, default);
        await _legalEntityRepository.DidNotReceiveWithAnyArgs()
            .AddAsync(default!, default);
    }

    [Fact]
    [Trait("RuleId", "RN-013")]
    public async Task Execute_DeveRetornarListaVazia_QuandoTermoNaoEhCnpjSemCorrespondencia()
    {
        SearchReturns();

        var response = await _useCase.ExecuteAsync(
            new SearchLegalEntitiesRequest("52998224725", "Insured"), CancellationToken.None);

        response.Items.Should().BeEmpty();
        await _bureauProvider.DidNotReceiveWithAnyArgs()
            .GetPersonComplementAsync(default!, default!, default, default);
    }

    [Fact]
    [Trait("RuleId", "RN-014")]
    public async Task Execute_DeveImportarDoBiro_QuandoCnpjNaoCadastrado()
    {
        SearchReturns();
        BureauReturns(HeadquartersCnpj, Complement());
        NatureExists();

        var response = await _useCase.ExecuteAsync(
            new SearchLegalEntitiesRequest(HeadquartersCnpj, "Insured"), CancellationToken.None);

        response.Items.Should().ContainSingle();
        response.Items[0].Cnpj.Should().Be(HeadquartersCnpj);
        response.Items[0].MainAddress.Should().NotBeNull();
        response.Items[0].MainAddress!.City.Should().Be("São Paulo");
        await _legalEntityRepository.Received(1)
            .AddAsync(Arg.Is<LegalEntity>(entity => entity.Cnpj == HeadquartersCnpj), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-014")]
    public async Task Execute_DeveRetornarVaziaComAviso_QuandoBiroNaoLocaliza()
    {
        SearchReturns();
        BureauReturns(HeadquartersCnpj, null);

        var response = await _useCase.ExecuteAsync(
            new SearchLegalEntitiesRequest(HeadquartersCnpj, "Insured"), CancellationToken.None);

        response.Items.Should().BeEmpty();
        response.Notice.Should().NotBeNullOrEmpty();
        await _legalEntityRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    [Trait("RuleId", "RN-014")]
    public async Task Execute_DeveRecusarImportacao_QuandoNaturezaJuridicaNaoCatalogada()
    {
        SearchReturns();
        BureauReturns(HeadquartersCnpj, Complement());
        _legalNatureRepository.GetByCodeAsync("2062", Arg.Any<CancellationToken>())
            .Returns((LegalNature?)null);

        var action = () => _useCase.ExecuteAsync(
            new SearchLegalEntitiesRequest(HeadquartersCnpj, "Insured"), CancellationToken.None);

        await action.Should().ThrowAsync<BusinessRuleException>();
        await _legalEntityRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    [Trait("RuleId", "RN-015")]
    public async Task Execute_DeveClassificarSetorPublico_QuandoNaturezaPublica()
    {
        SearchReturns();
        BureauReturns(HeadquartersCnpj, Complement());
        NatureExists(isPrivate: false);

        var response = await _useCase.ExecuteAsync(
            new SearchLegalEntitiesRequest(HeadquartersCnpj, "Insured"), CancellationToken.None);

        response.Items.Should().ContainSingle();
        response.Items[0].IsPrivateSector.Should().BeFalse();
    }

    [Fact]
    [Trait("RuleId", "RN-016")]
    public async Task Execute_DeveFiltrarSomenteMatrizes_QuandoContextoTomador()
    {
        SearchReturns(Item(HeadquartersCnpj));

        await _useCase.ExecuteAsync(
            new SearchLegalEntitiesRequest("Alfa", "PolicyHolder"), CancellationToken.None);

        await _legalEntityRepository.Received(1).SearchByNameOrCnpjAsync(
            "Alfa", null, true, Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-016")]
    public async Task Execute_DeveResolverMatrizDaBase_QuandoTomadorInformaFilial()
    {
        SearchReturns();
        _legalEntityRepository.GetByCnpjAsync(HeadquartersCnpj, Arg.Any<CancellationToken>())
            .Returns(Item(HeadquartersCnpj));

        var response = await _useCase.ExecuteAsync(
            new SearchLegalEntitiesRequest(BranchCnpj, "PolicyHolder"), CancellationToken.None);

        response.Items.Should().ContainSingle(item => item.Cnpj == HeadquartersCnpj);
        response.Items[0].PreSelectedBranchCnpj.Should().Be(BranchCnpj);
        await _bureauProvider.DidNotReceiveWithAnyArgs()
            .GetPersonComplementAsync(default!, default!, default, default);
    }

    [Fact]
    [Trait("RuleId", "RN-016")]
    public async Task Execute_DeveImportarMatrizDoBiro_QuandoFilialSemMatrizCadastrada()
    {
        SearchReturns();
        _legalEntityRepository.GetByCnpjAsync(HeadquartersCnpj, Arg.Any<CancellationToken>())
            .Returns((LegalEntitySearchItemDto?)null);
        BureauReturns(HeadquartersCnpj, Complement());
        NatureExists();

        var response = await _useCase.ExecuteAsync(
            new SearchLegalEntitiesRequest(BranchCnpj, "PolicyHolder"), CancellationToken.None);

        response.Items.Should().ContainSingle(item => item.Cnpj == HeadquartersCnpj);
        response.Items[0].PreSelectedBranchCnpj.Should().Be(BranchCnpj);
        await _bureauProvider.Received(1).GetPersonComplementAsync(
            HeadquartersCnpj, "Tomador", EBureau.ReceitaWS, Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-016")]
    public async Task Execute_DeveRetornarVaziaComAviso_QuandoMatrizNaoLocalizadaNoBiro()
    {
        SearchReturns();
        _legalEntityRepository.GetByCnpjAsync(HeadquartersCnpj, Arg.Any<CancellationToken>())
            .Returns((LegalEntitySearchItemDto?)null);
        BureauReturns(HeadquartersCnpj, null);

        var response = await _useCase.ExecuteAsync(
            new SearchLegalEntitiesRequest(BranchCnpj, "PolicyHolder"), CancellationToken.None);

        response.Items.Should().BeEmpty();
        response.Notice.Should().NotBeNullOrEmpty();
    }
}
