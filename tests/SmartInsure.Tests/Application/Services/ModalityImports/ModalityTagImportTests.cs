using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SmartInsure.Application.UseCase.Services.ModalityImports;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Tests.Application.Services.ModalityImports;

/// <summary>RN-040/RN-041/RN-042 — importação de Tag e Cláusulas particulares no ciclo de catálogo.</summary>
public class ModalityTagImportTests
{
    private static readonly DateTime Now = new(2026, 7, 23, 5, 0, 0, DateTimeKind.Utc);
    private static readonly Guid InsurerId = Guid.CreateVersion7();

    private readonly IBrokerageInsurerEnablementRepository _enablements =
        Substitute.For<IBrokerageInsurerEnablementRepository>();
    private readonly IImportedGroupRepository _groups = Substitute.For<IImportedGroupRepository>();
    private readonly IImportedModalityRepository _modalities = Substitute.For<IImportedModalityRepository>();
    private readonly IModalityRepository _catalog = Substitute.For<IModalityRepository>();
    private readonly IImportedModalityTagRepository _tags = Substitute.For<IImportedModalityTagRepository>();
    private readonly IImportedModalityParticularClauseRepository _clauses =
        Substitute.For<IImportedModalityParticularClauseRepository>();
    private readonly ICalculationEngine _engine = Substitute.For<ICalculationEngine>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private ImportedModality? _added;

    private ModalityImporter BuildImporter()
    {
        var provider = new ServiceCollection()
            .AddKeyedSingleton(ECalculationEngine.PlugV2, _engine)
            .BuildServiceProvider();

        _clauses.ListByImportedModalityAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ImportedModalityParticularClause>());

        return new ModalityImporter(
            _enablements, _groups, _modalities, _catalog, _tags, _clauses, provider, _unitOfWork);
    }

    private void GivenSingleModalityCatalog()
    {
        _enablements.ListActiveForImportAsync(Arg.Any<CancellationToken>()).Returns(
            new List<ActiveEnablementImportDto>
            {
                new(Guid.CreateVersion7(), Guid.CreateVersion7(), "34060267000196",
                    InsurerId, "ref-1", "PlugV2", "{\"baseUrl\":\"https://x\",\"key\":\"k\"}"),
            });

        _engine.GetGroupAndModalitiesAsync(Arg.Any<string?>(), "34060267000196", Arg.Any<CancellationToken>())
            .Returns(new ImportedCatalogResult(
            [
                new ImportedInsurerCatalog("ref-1", "Essor", true,
                [
                    new ImportedModalityData("m-1", "Garantia", ESuretyBranch.Public, "1", "Judicial", "g-1", "Financeira", null, "{}"),
                ]),
            ]));

        _groups.GetByInsurerAndSourceAsync(InsurerId, "g-1", Arg.Any<CancellationToken>()).Returns((ImportedGroup?)null);
        _modalities.GetByInsurerAndSourceAsync(InsurerId, "m-1", Arg.Any<CancellationToken>()).Returns((ImportedModality?)null);
        _catalog.GetByGlobalExternalIdAsync("1", Arg.Any<CancellationToken>()).Returns((Modality?)null);
        _modalities.ListActiveByInsurerAsync(InsurerId, Arg.Any<CancellationToken>()).Returns(new List<ImportedModality>());
        _modalities.AddAsync(Arg.Do<ImportedModality>(m => _added = m), Arg.Any<CancellationToken>());
    }

    private void GivenModalityObject(ModalityObjectResult result)
        => _engine.GetModalityObjectAsync(Arg.Any<string?>(), "34060267000196", "m-1", Arg.Any<CancellationToken>())
            .Returns(result);

    [Fact]
    [Trait("RuleId", "RN-040")]
    public async Task DeveCriarTag_QuandoObjetoTrazJsonTag()
    {
        GivenSingleModalityCatalog();
        GivenModalityObject(new ModalityObjectResult(false, "{\"campo\":1}", "obj", Array.Empty<ModalityClauseData>()));
        _tags.GetByImportedModalityAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ImportedModalityTag?)null);

        await BuildImporter().RunAsync(Now, CancellationToken.None);

        await _tags.Received(1).AddAsync(
            Arg.Is<ImportedModalityTag>(t => t.JsonTag == "{\"campo\":1}" && t.ImportedModalityId == _added!.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-040")]
    public async Task NaoDeveSobrescreverTag_QuandoObjetoSemJsonTag_EInativaExistente()
    {
        GivenSingleModalityCatalog();
        GivenModalityObject(new ModalityObjectResult(false, null, null, Array.Empty<ModalityClauseData>()));
        var existing = ImportedModalityTag.Create(Guid.CreateVersion7(), "{\"v\":1}", "x");
        _tags.GetByImportedModalityAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(existing);

        await BuildImporter().RunAsync(Now, CancellationToken.None);

        await _tags.DidNotReceive().AddAsync(Arg.Any<ImportedModalityTag>(), Arg.Any<CancellationToken>());
        existing.Status.Should().Be(EImportedModalityTagStatus.Inactive); // RN-042
        existing.JsonTag.Should().Be("{\"v\":1}"); // conteúdo preservado (RN-040)
    }

    [Fact]
    [Trait("RuleId", "RN-041")]
    public async Task DeveCriarClausulas_PorChaveExterna()
    {
        GivenSingleModalityCatalog();
        GivenModalityObject(new ModalityObjectResult(false, "{}", null,
            new List<ModalityClauseData> { new("123", "Retencao", "t", "{}"), new("456", "Outra", null, null) }));
        _tags.GetByImportedModalityAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ImportedModalityTag?)null);

        await BuildImporter().RunAsync(Now, CancellationToken.None);

        await _clauses.Received(2).AddAsync(Arg.Any<ImportedModalityParticularClause>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-041")]
    public async Task DeveAtualizarClausulaExistente_SemDuplicar()
    {
        GivenSingleModalityCatalog();
        GivenModalityObject(new ModalityObjectResult(false, "{}", null,
            new List<ModalityClauseData> { new("123", "Nova", "t2", "{}") }));
        _tags.GetByImportedModalityAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ImportedModalityTag?)null);
        var existing = ImportedModalityParticularClause.Create(Guid.CreateVersion7(), "123", "Old", null, null);
        var importer = BuildImporter();
        // Override do default (Array.Empty) configurado dentro de BuildImporter(): precisa vir depois dela,
        // senão o NSubstitute reconfigura a mesma correspondência de argumentos de volta para vazio.
        _clauses.ListByImportedModalityAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new List<ImportedModalityParticularClause> { existing });

        await importer.RunAsync(Now, CancellationToken.None);

        await _clauses.DidNotReceive().AddAsync(Arg.Any<ImportedModalityParticularClause>(), Arg.Any<CancellationToken>());
        existing.Name.Should().Be("Nova");
    }

    [Fact]
    [Trait("RuleId", "RN-042")]
    public async Task DeveInativarClausulaAusente_EmConsultaBemSucedida()
    {
        GivenSingleModalityCatalog();
        GivenModalityObject(new ModalityObjectResult(false, "{}", null, Array.Empty<ModalityClauseData>()));
        _tags.GetByImportedModalityAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ImportedModalityTag?)null);
        var orphan = ImportedModalityParticularClause.Create(Guid.CreateVersion7(), "999", "Sumiu", null, null);
        var importer = BuildImporter();
        // Override do default (Array.Empty) configurado dentro de BuildImporter(): precisa vir depois dela,
        // senão o NSubstitute reconfigura a mesma correspondência de argumentos de volta para vazio.
        _clauses.ListByImportedModalityAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new List<ImportedModalityParticularClause> { orphan });

        await importer.RunAsync(Now, CancellationToken.None);

        orphan.Status.Should().Be(EImportedModalityClauseStatus.Inactive);
    }

    [Fact]
    [Trait("RuleId", "RN-042")]
    public async Task NaoDeveInativarNada_QuandoConsultaDaModalidadeFalha()
    {
        GivenSingleModalityCatalog();
        GivenModalityObject(new ModalityObjectResult(true, null, null, Array.Empty<ModalityClauseData>()));
        var tag = ImportedModalityTag.Create(Guid.CreateVersion7(), "{\"v\":1}", null);
        _tags.GetByImportedModalityAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(tag);
        var clause = ImportedModalityParticularClause.Create(Guid.CreateVersion7(), "123", "C", null, null);
        _clauses.ListByImportedModalityAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new List<ImportedModalityParticularClause> { clause });

        await BuildImporter().RunAsync(Now, CancellationToken.None);

        tag.Status.Should().Be(EImportedModalityTagStatus.Active);       // RN-042: falha não inativa
        clause.Status.Should().Be(EImportedModalityClauseStatus.Active);
        await _tags.DidNotReceive().AddAsync(Arg.Any<ImportedModalityTag>(), Arg.Any<CancellationToken>());
    }
}
