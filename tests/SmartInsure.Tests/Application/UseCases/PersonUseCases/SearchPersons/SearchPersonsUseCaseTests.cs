using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.Services.PersonImports;
using SmartInsure.Application.UseCase.UseCases.PersonUseCases.SearchPersons;
using SmartInsure.Application.UseCase.UseCases.PersonUseCases.SearchPersons.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Abstractions.Services.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.PersonUseCases.SearchPersons;

/// <summary>
/// RN-013..RN-016 — busca de Pessoa por nome ou documento, com importação
/// do Birô e resolução de matriz para tomador.
/// </summary>
public class SearchPersonsUseCaseTests
{
    private const string HeadquartersCnpj = "11444777000161";
    private const string BranchCnpj = "11444777000242";
    private const string Cpf = "52998224725";

    private readonly IPersonRepository _personRepository = Substitute.For<IPersonRepository>();

    private readonly ILegalNatureRepository _legalNatureRepository =
        Substitute.For<ILegalNatureRepository>();

    private readonly IBureauProvider _bureauProvider = Substitute.For<IBureauProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly SearchPersonsUseCase _useCase;

    public SearchPersonsUseCaseTests()
        => _useCase = new SearchPersonsUseCase(
            _personRepository,
            new PersonBureauImporter(_legalNatureRepository, _bureauProvider),
            _unitOfWork);

    private static PersonSearchItemDto Item(
        string documentNumber, string name = "Alfa Ltda", string type = "J", bool? isPrivate = true)
        => new(Guid.NewGuid(), documentNumber, name, null, type, isPrivate, [], null);

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

    private void SearchReturns(params PersonSearchItemDto[] items)
        => _personRepository.SearchByNameOrDocumentAsync(
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
            new SearchPersonsRequest("Alfa", "Insured"), CancellationToken.None);

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
            new SearchPersonsRequest(HeadquartersCnpj, "Insured"), CancellationToken.None);

        response.Items.Should().ContainSingle(item => item.DocumentNumber == HeadquartersCnpj);
        await _bureauProvider.DidNotReceiveWithAnyArgs()
            .GetPersonComplementAsync(default!, default!, default, default);
        await _personRepository.DidNotReceiveWithAnyArgs()
            .AddAsync(default!, default);
    }

    [Fact]
    [Trait("RuleId", "RN-013")]
    public async Task Execute_DeveRetornarPessoaFisicaDaBase_QuandoCpfJaCadastrado()
    {
        SearchReturns(Item(Cpf, "Maria Silva", "F", isPrivate: null));

        var response = await _useCase.ExecuteAsync(
            new SearchPersonsRequest(Cpf, "Insured"), CancellationToken.None);

        response.Items.Should().ContainSingle(item => item.DocumentNumber == Cpf);
        response.Items[0].Type.Should().Be("F");
        response.Items[0].IsPrivateSector.Should().BeNull();
        await _personRepository.Received(1).SearchByNameOrDocumentAsync(
            Cpf, Cpf, false, Arg.Any<CancellationToken>());
        await _bureauProvider.DidNotReceiveWithAnyArgs()
            .GetPersonComplementAsync(default!, default!, default, default);
    }

    [Fact]
    [Trait("RuleId", "RN-013")]
    public async Task Execute_DeveRetornarListaVaziaSemBiro_QuandoCpfNaoCadastrado()
    {
        SearchReturns();

        var response = await _useCase.ExecuteAsync(
            new SearchPersonsRequest(Cpf, "Insured"), CancellationToken.None);

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
            new SearchPersonsRequest(HeadquartersCnpj, "Insured"), CancellationToken.None);

        response.Items.Should().ContainSingle();
        response.Items[0].DocumentNumber.Should().Be(HeadquartersCnpj);
        response.Items[0].Type.Should().Be("J");
        response.Items[0].MainAddress.Should().NotBeNull();
        response.Items[0].MainAddress!.City.Should().Be("São Paulo");
        await _personRepository.Received(1)
            .AddAsync(
                Arg.Is<Person>(person => person.DocumentNumber == HeadquartersCnpj
                    && person.Type == EPersonType.J),
                Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-014")]
    public async Task Execute_DeveRetornarVaziaComAviso_QuandoBiroNaoLocaliza()
    {
        SearchReturns();
        BureauReturns(HeadquartersCnpj, null);

        var response = await _useCase.ExecuteAsync(
            new SearchPersonsRequest(HeadquartersCnpj, "Insured"), CancellationToken.None);

        response.Items.Should().BeEmpty();
        response.Notice.Should().NotBeNullOrEmpty();
        await _personRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
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
            new SearchPersonsRequest(HeadquartersCnpj, "Insured"), CancellationToken.None);

        await action.Should().ThrowAsync<BusinessRuleException>();
        await _personRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
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
            new SearchPersonsRequest(HeadquartersCnpj, "Insured"), CancellationToken.None);

        response.Items.Should().ContainSingle();
        response.Items[0].IsPrivateSector.Should().BeFalse();
    }

    [Fact]
    [Trait("RuleId", "RN-016")]
    public async Task Execute_DeveFiltrarSomenteMatrizes_QuandoContextoTomador()
    {
        SearchReturns(Item(HeadquartersCnpj));

        await _useCase.ExecuteAsync(
            new SearchPersonsRequest("Alfa", "PolicyHolder"), CancellationToken.None);

        await _personRepository.Received(1).SearchByNameOrDocumentAsync(
            "Alfa", null, true, Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-016")]
    public async Task Execute_DeveResolverMatrizDaBase_QuandoTomadorInformaFilial()
    {
        SearchReturns();
        _personRepository.GetByDocumentNumberAsync(HeadquartersCnpj, Arg.Any<CancellationToken>())
            .Returns(Item(HeadquartersCnpj));

        var response = await _useCase.ExecuteAsync(
            new SearchPersonsRequest(BranchCnpj, "PolicyHolder"), CancellationToken.None);

        response.Items.Should().ContainSingle(item => item.DocumentNumber == HeadquartersCnpj);
        response.Items[0].PreSelectedBranchDocumentNumber.Should().Be(BranchCnpj);
        await _bureauProvider.DidNotReceiveWithAnyArgs()
            .GetPersonComplementAsync(default!, default!, default, default);
    }

    [Fact]
    [Trait("RuleId", "RN-016")]
    public async Task Execute_DeveImportarMatrizDoBiro_QuandoFilialSemMatrizCadastrada()
    {
        SearchReturns();
        _personRepository.GetByDocumentNumberAsync(HeadquartersCnpj, Arg.Any<CancellationToken>())
            .Returns((PersonSearchItemDto?)null);
        BureauReturns(HeadquartersCnpj, Complement());
        NatureExists();

        var response = await _useCase.ExecuteAsync(
            new SearchPersonsRequest(BranchCnpj, "PolicyHolder"), CancellationToken.None);

        response.Items.Should().ContainSingle(item => item.DocumentNumber == HeadquartersCnpj);
        response.Items[0].PreSelectedBranchDocumentNumber.Should().Be(BranchCnpj);
        await _bureauProvider.Received(1).GetPersonComplementAsync(
            HeadquartersCnpj, "Tomador", EBureau.ReceitaWS, Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-017")]
    public async Task Execute_DeveVincularPapel_QuandoDevolvidaPorDocumento()
    {
        SearchReturns(Item(HeadquartersCnpj));
        var tracked = Person.Create(HeadquartersCnpj, "Alfa Ltda", null, Guid.NewGuid());
        _personRepository.GetTrackedByDocumentNumberAsync(HeadquartersCnpj, Arg.Any<CancellationToken>())
            .Returns(tracked);

        var response = await _useCase.ExecuteAsync(
            new SearchPersonsRequest(HeadquartersCnpj, "Insured"), CancellationToken.None);

        tracked.Roles.Should().ContainSingle(role => role.Role == EPersonRole.Insured);
        response.Items[0].Roles.Should().Contain("Insured");
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-017")]
    public async Task Execute_NaoDeveVincularPapel_QuandoBuscaPorNome()
    {
        SearchReturns(Item("12345678000195"));

        await _useCase.ExecuteAsync(
            new SearchPersonsRequest("Alfa", "Insured"), CancellationToken.None);

        await _personRepository.DidNotReceiveWithAnyArgs()
            .GetTrackedByDocumentNumberAsync(default!, default);
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    [Trait("RuleId", "RN-017")]
    public async Task Execute_DeveImportarComPapelDoContexto_QuandoImportaDoBiro()
    {
        SearchReturns();
        BureauReturns(HeadquartersCnpj, Complement());
        NatureExists();

        var response = await _useCase.ExecuteAsync(
            new SearchPersonsRequest(HeadquartersCnpj, "Broker"), CancellationToken.None);

        response.Items[0].Roles.Should().ContainSingle().Which.Should().Be("Broker");
        await _personRepository.Received(1).AddAsync(
            Arg.Is<Person>(person => person.Roles.Count == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-016")]
    public async Task Execute_DeveRetornarVaziaComAviso_QuandoMatrizNaoLocalizadaNoBiro()
    {
        SearchReturns();
        _personRepository.GetByDocumentNumberAsync(HeadquartersCnpj, Arg.Any<CancellationToken>())
            .Returns((PersonSearchItemDto?)null);
        BureauReturns(HeadquartersCnpj, null);

        var response = await _useCase.ExecuteAsync(
            new SearchPersonsRequest(BranchCnpj, "PolicyHolder"), CancellationToken.None);

        response.Items.Should().BeEmpty();
        response.Notice.Should().NotBeNullOrEmpty();
    }
}
