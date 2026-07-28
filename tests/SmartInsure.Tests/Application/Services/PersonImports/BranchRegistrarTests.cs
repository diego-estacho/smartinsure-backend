using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.Services.PersonImports;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.Services.PersonImports;

/// <summary>RN-052 — cadastro em cadeia da Filial: resolve a matriz, importa quando ausente e vincula.</summary>
public class BranchRegistrarTests
{
    private const string HeadquartersCnpj = "11444777000161";
    private const string BranchCnpj = "11444777000242";

    private readonly IPersonRepository _personRepository = Substitute.For<IPersonRepository>();
    private readonly IPersonBureauImporter _personBureauImporter = Substitute.For<IPersonBureauImporter>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly BranchRegistrar _registrar;

    public BranchRegistrarTests()
        => _registrar = new BranchRegistrar(_personRepository, _personBureauImporter, _unitOfWork);

    private static Person NewPerson(string cnpj)
        => Person.Create(cnpj, "Alfa Ltda", "Alfa", Guid.NewGuid());

    private void TrackedReturns(string cnpj, Person? person)
        => _personRepository.GetTrackedByDocumentNumberAsync(cnpj, Arg.Any<CancellationToken>())
            .Returns(person);

    private void ImportReturns(string cnpj, Person? person)
        => _personBureauImporter.ImportLegalPersonAsync(cnpj, null, Arg.Any<CancellationToken>())
            .Returns(person is null ? null : new PersonBureauImport(person, true));

    [Fact]
    [Trait("RuleId", "RN-052")]
    public async Task RegisterAsync_MatrizEFilialAusentes_DeveImportarAsDuasEVincular()
    {
        var headquarters = NewPerson(HeadquartersCnpj);
        var branch = NewPerson(BranchCnpj);
        TrackedReturns(HeadquartersCnpj, null);
        TrackedReturns(BranchCnpj, null);
        ImportReturns(HeadquartersCnpj, headquarters);
        ImportReturns(BranchCnpj, branch);

        var result = await _registrar.RegisterAsync(BranchCnpj, CancellationToken.None);

        result.Should().NotBeNull();
        result!.HeadquartersId.Should().Be(headquarters.Id);
        result.BranchId.Should().Be(branch.Id);
        result.Notice.Should().BeNull();
        branch.HeadquartersPersonId.Should().Be(headquarters.Id);
        await _personRepository.Received(1).AddAsync(headquarters, Arg.Any<CancellationToken>());
        await _personRepository.Received(1).AddAsync(branch, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(3).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-052")]
    public async Task RegisterAsync_MatrizExistente_NaoDeveConsultarBiroParaAMatriz()
    {
        var headquarters = NewPerson(HeadquartersCnpj);
        var branch = NewPerson(BranchCnpj);
        TrackedReturns(HeadquartersCnpj, headquarters);
        TrackedReturns(BranchCnpj, null);
        ImportReturns(BranchCnpj, branch);

        var result = await _registrar.RegisterAsync(BranchCnpj, CancellationToken.None);

        result.Should().NotBeNull();
        result!.HeadquartersId.Should().Be(headquarters.Id);
        await _personBureauImporter.DidNotReceive().ImportLegalPersonAsync(
            HeadquartersCnpj, Arg.Any<EPersonRole?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-052")]
    public async Task RegisterAsync_BiroSemMatriz_NaoDeveGravarNada()
    {
        TrackedReturns(HeadquartersCnpj, null);
        ImportReturns(HeadquartersCnpj, null);

        var result = await _registrar.RegisterAsync(BranchCnpj, CancellationToken.None);

        result.Should().BeNull();
        await _personRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
        await _personBureauImporter.DidNotReceive().ImportLegalPersonAsync(
            BranchCnpj, Arg.Any<EPersonRole?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-052")]
    public async Task RegisterAsync_BiroSemFilial_DevePreservarMatrizEAvisar()
    {
        var headquarters = NewPerson(HeadquartersCnpj);
        TrackedReturns(HeadquartersCnpj, null);
        TrackedReturns(BranchCnpj, null);
        ImportReturns(HeadquartersCnpj, headquarters);
        ImportReturns(BranchCnpj, null);

        var result = await _registrar.RegisterAsync(BranchCnpj, CancellationToken.None);

        result.Should().NotBeNull();
        result!.HeadquartersId.Should().Be(headquarters.Id);
        result.BranchId.Should().BeNull();
        result.Notice.Should().NotBeNullOrEmpty();
        await _personRepository.Received(1).AddAsync(headquarters, Arg.Any<CancellationToken>());
        await _personRepository.Received(1).AddAsync(Arg.Any<Person>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-052")]
    public async Task RegisterAsync_FilialJaVinculada_NaoDeveConsultarBiro()
    {
        var headquarters = NewPerson(HeadquartersCnpj);
        var branch = NewPerson(BranchCnpj);
        branch.LinkToHeadquarters(headquarters);
        TrackedReturns(HeadquartersCnpj, headquarters);
        TrackedReturns(BranchCnpj, branch);

        var result = await _registrar.RegisterAsync(BranchCnpj, CancellationToken.None);

        result.Should().NotBeNull();
        result!.HeadquartersId.Should().Be(headquarters.Id);
        result.BranchId.Should().Be(branch.Id);
        result.Notice.Should().BeNull();
        await _personBureauImporter.DidNotReceiveWithAnyArgs().ImportLegalPersonAsync(
            default!, default, default);
        await _personRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    [Trait("RuleId", "RN-052")]
    public async Task RegisterAsync_PessoaExistenteSemVinculo_DeveApenasVincular()
    {
        var headquarters = NewPerson(HeadquartersCnpj);
        var branch = NewPerson(BranchCnpj);
        TrackedReturns(HeadquartersCnpj, headquarters);
        TrackedReturns(BranchCnpj, branch);

        var result = await _registrar.RegisterAsync(BranchCnpj, CancellationToken.None);

        result.Should().NotBeNull();
        result!.HeadquartersId.Should().Be(headquarters.Id);
        result.BranchId.Should().Be(branch.Id);
        result.Notice.Should().BeNull();
        branch.HeadquartersPersonId.Should().Be(headquarters.Id);
        await _personBureauImporter.DidNotReceiveWithAnyArgs().ImportLegalPersonAsync(
            default!, default, default);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-052")]
    public async Task RegisterAsync_CnpjDeMatriz_DeveSerRecusado()
    {
        var action = () => _registrar.RegisterAsync(HeadquartersCnpj, CancellationToken.None);

        await action.Should().ThrowAsync<BusinessRuleException>();
        await _personRepository.DidNotReceiveWithAnyArgs()
            .GetTrackedByDocumentNumberAsync(default!, default);
        await _personBureauImporter.DidNotReceiveWithAnyArgs().ImportLegalPersonAsync(
            default!, default, default);
    }
}
