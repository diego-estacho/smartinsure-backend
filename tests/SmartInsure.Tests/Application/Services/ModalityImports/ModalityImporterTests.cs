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

/// <summary>RN-033/RN-034/RN-035/RN-038 — importação de modalidades pelo Motor de Cálculo (ADR-061).</summary>
public class ModalityImporterTests
{
    private static readonly DateTime Now = new(2026, 7, 22, 3, 0, 0, DateTimeKind.Utc);
    private static readonly Guid InsurerId = Guid.CreateVersion7();

    private readonly IBrokerageInsurerEnablementRepository _enablements =
        Substitute.For<IBrokerageInsurerEnablementRepository>();

    private readonly IImportedGroupRepository _groups = Substitute.For<IImportedGroupRepository>();
    private readonly IImportedModalityRepository _modalities = Substitute.For<IImportedModalityRepository>();
    private readonly IModalityRepository _catalog = Substitute.For<IModalityRepository>();
    private readonly IImportedModalityTagRepository _tags = Substitute.For<IImportedModalityTagRepository>();
    private readonly IImportedModalityParticularClauseRepository _clauses = Substitute.For<IImportedModalityParticularClauseRepository>();
    private readonly ICalculationEngine _engine = Substitute.For<ICalculationEngine>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private ModalityImporter BuildImporter()
    {
        var provider = new ServiceCollection()
            .AddKeyedSingleton(ECalculationEngine.PlugV2, _engine)
            .BuildServiceProvider();

        // Passo de objeto (U5) benigno nos testes de modalidade: objeto bem-sucedido vazio, sem cláusulas locais.
        _engine.GetModalityObjectAsync(Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ModalityObjectResult(false, null, null, Array.Empty<ModalityClauseData>()));
        _clauses.ListByImportedModalityAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ImportedModalityParticularClause>());

        return new ModalityImporter(
            _enablements, _groups, _modalities, _catalog, _tags, _clauses, provider, _unitOfWork);
    }

    private void GivenActiveEnablement()
        => _enablements.ListActiveForImportAsync(Arg.Any<CancellationToken>()).Returns(
            new List<ActiveEnablementImportDto>
            {
                new(Guid.CreateVersion7(), Guid.CreateVersion7(), "34060267000196",
                    InsurerId, "ref-1", "PlugV2", "{\"baseUrl\":\"https://x\",\"key\":\"k\"}"),
            });

    private void GivenCatalog(bool isSuccess, params ImportedModalityData[] modalities)
        => _engine.GetGroupAndModalitiesAsync(Arg.Any<string?>(), "34060267000196", Arg.Any<CancellationToken>())
            .Returns(new ImportedCatalogResult(
            [
                new ImportedInsurerCatalog("ref-1", "Essor", isSuccess, modalities),
            ]));

    private static ImportedModalityData Modality(string sourceId, string engineId, ESuretyBranch branch)
        => new(sourceId, "Garantia", branch, engineId, "Execução", "g-1", "Financeira", "GARANTIA_FINANCEIRA", "{}");

    [Fact]
    [Trait("RuleId", "RN-035")]
    public async Task Run_DeveCriarModalidadeGlobalEVincularAutomaticamente_QuandoSucesso()
    {
        GivenActiveEnablement();
        GivenCatalog(isSuccess: true, Modality("m-1", "1", ESuretyBranch.Public));
        _groups.GetByInsurerAndSourceAsync(InsurerId, "g-1", Arg.Any<CancellationToken>()).Returns((ImportedGroup?)null);
        _modalities.GetByInsurerAndSourceAsync(InsurerId, "m-1", Arg.Any<CancellationToken>()).Returns((ImportedModality?)null);
        _catalog.GetByGlobalExternalIdAsync("1", Arg.Any<CancellationToken>()).Returns((Modality?)null);
        _modalities.ListActiveByInsurerAsync(InsurerId, Arg.Any<CancellationToken>()).Returns(new List<ImportedModality>());

        ImportedModality? added = null;
        await _modalities.AddAsync(Arg.Do<ImportedModality>(m => added = m), Arg.Any<CancellationToken>());

        var summary = await BuildImporter().RunAsync(Now, CancellationToken.None);

        summary.InsurersSucceeded.Should().Be(1);
        await _catalog.Received(1).AddAsync(
            Arg.Is<Modality>(m => m.GlobalModalityExternalId == "1"), Arg.Any<CancellationToken>());
        await _modalities.Received(1).AddAsync(Arg.Any<ImportedModality>(), Arg.Any<CancellationToken>());
        added.Should().NotBeNull();
        added!.ModalityId.Should().NotBeNull();
        added.ModalityLinkSource.Should().Be(EModalityLinkSource.Automatic);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-035")]
    public async Task Run_DeveReusarModalidadeExistente_QuandoIdGlobalJaConhecido()
    {
        GivenActiveEnablement();
        GivenCatalog(isSuccess: true, Modality("m-1", "1", ESuretyBranch.Public));
        _groups.GetByInsurerAndSourceAsync(InsurerId, "g-1", Arg.Any<CancellationToken>()).Returns((ImportedGroup?)null);
        _modalities.GetByInsurerAndSourceAsync(InsurerId, "m-1", Arg.Any<CancellationToken>()).Returns((ImportedModality?)null);
        _catalog.GetByGlobalExternalIdAsync("1", Arg.Any<CancellationToken>())
            .Returns(SmartInsure.Core.Entities.Modality.CreateFromGlobal("1", "Execução"));
        _modalities.ListActiveByInsurerAsync(InsurerId, Arg.Any<CancellationToken>()).Returns(new List<ImportedModality>());

        await BuildImporter().RunAsync(Now, CancellationToken.None);

        // Modalidade já existe pelo id global — não cria outra (evita violar o índice único).
        await _catalog.DidNotReceive().AddAsync(Arg.Any<Modality>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-035")]
    public async Task Run_DeveCriarModalidadeUmaVez_QuandoImportadasCompartilhamOIdGlobal()
    {
        GivenActiveEnablement();
        GivenCatalog(
            isSuccess: true,
            new ImportedModalityData("m-1", "Garantia A", ESuretyBranch.Public, "1", "Judicial", "g-1", "Financeira", null, "{}"),
            new ImportedModalityData("m-2", "Garantia B", ESuretyBranch.Private, "1", "Judicial", "g-1", "Financeira", null, "{}"));
        _groups.GetByInsurerAndSourceAsync(InsurerId, "g-1", Arg.Any<CancellationToken>()).Returns((ImportedGroup?)null);
        _modalities.GetByInsurerAndSourceAsync(InsurerId, Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((ImportedModality?)null);
        _catalog.GetByGlobalExternalIdAsync("1", Arg.Any<CancellationToken>()).Returns((Modality?)null);
        _modalities.ListActiveByInsurerAsync(InsurerId, Arg.Any<CancellationToken>()).Returns(new List<ImportedModality>());

        await BuildImporter().RunAsync(Now, CancellationToken.None);

        // Mesmo id de Modalidade Global compartilhado no lote: a Modalidade é criada uma única vez.
        await _catalog.Received(1).AddAsync(Arg.Any<Modality>(), Arg.Any<CancellationToken>());
        await _modalities.Received(2).AddAsync(Arg.Any<ImportedModality>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-035")]
    public async Task Run_NaoDeveVincular_QuandoImportadaSemIdGlobal()
    {
        GivenActiveEnablement();
        GivenCatalog(
            isSuccess: true,
            new ImportedModalityData("m-1", "Sem global", ESuretyBranch.Public, null, null, "g-1", "Financeira", null, "{}"));
        _groups.GetByInsurerAndSourceAsync(InsurerId, "g-1", Arg.Any<CancellationToken>()).Returns((ImportedGroup?)null);
        _modalities.GetByInsurerAndSourceAsync(InsurerId, "m-1", Arg.Any<CancellationToken>()).Returns((ImportedModality?)null);
        _modalities.ListActiveByInsurerAsync(InsurerId, Arg.Any<CancellationToken>()).Returns(new List<ImportedModality>());

        ImportedModality? added = null;
        await _modalities.AddAsync(Arg.Do<ImportedModality>(m => added = m), Arg.Any<CancellationToken>());

        await BuildImporter().RunAsync(Now, CancellationToken.None);

        await _catalog.DidNotReceive().AddAsync(Arg.Any<Modality>(), Arg.Any<CancellationToken>());
        added.Should().NotBeNull();
        added!.ModalityId.Should().BeNull();
    }

    [Fact]
    [Trait("RuleId", "RN-033")]
    public async Task Run_DeveCriarGrupoUmaVez_QuandoModalidadesCompartilhamOGrupo()
    {
        GivenActiveEnablement();
        GivenCatalog(
            isSuccess: true,
            Modality("m-1", "1", ESuretyBranch.Public),
            Modality("m-2", "2", ESuretyBranch.Private)); // mesmo GroupSourceId "g-1"
        _groups.GetByInsurerAndSourceAsync(InsurerId, "g-1", Arg.Any<CancellationToken>()).Returns((ImportedGroup?)null);
        _modalities.GetByInsurerAndSourceAsync(InsurerId, Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((ImportedModality?)null);
        _catalog.GetByGlobalExternalIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Modality?)null);
        _modalities.ListActiveByInsurerAsync(InsurerId, Arg.Any<CancellationToken>()).Returns(new List<ImportedModality>());

        await BuildImporter().RunAsync(Now, CancellationToken.None);

        // Grupo compartilhado é criado uma única vez (evita violar o índice único no commit).
        await _groups.Received(1).AddAsync(Arg.Any<ImportedGroup>(), Arg.Any<CancellationToken>());
        await _modalities.Received(2).AddAsync(Arg.Any<ImportedModality>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-035")]
    public async Task Run_DevePreservarOverrideManual_NaReimportacao()
    {
        GivenActiveEnablement();
        GivenCatalog(isSuccess: true, Modality("m-1", "1", ESuretyBranch.Public));
        _groups.GetByInsurerAndSourceAsync(InsurerId, "g-1", Arg.Any<CancellationToken>()).Returns((ImportedGroup?)null);

        // Importada já existente, com override Manual apontando para outra Modalidade.
        var manualTarget = Guid.CreateVersion7();
        var existing = ImportedModality.Create(
            InsurerId, "m-1", "Garantia", ESuretyBranch.Public, "1", "Execução", null, "{}", Now);
        existing.LinkToModality(manualTarget, EModalityLinkSource.Manual);
        _modalities.GetByInsurerAndSourceAsync(InsurerId, "m-1", Arg.Any<CancellationToken>()).Returns(existing);
        _catalog.GetByGlobalExternalIdAsync("1", Arg.Any<CancellationToken>())
            .Returns(SmartInsure.Core.Entities.Modality.CreateFromGlobal("1", "Execução"));
        _modalities.ListActiveByInsurerAsync(InsurerId, Arg.Any<CancellationToken>())
            .Returns(new List<ImportedModality> { existing });

        await BuildImporter().RunAsync(Now, CancellationToken.None);

        existing.ModalityId.Should().Be(manualTarget);
        existing.ModalityLinkSource.Should().Be(EModalityLinkSource.Manual);
    }

    [Fact]
    [Trait("RuleId", "RN-038")]
    public async Task Run_NaoDeveImportarNemDesativar_QuandoSeguradoraFalha()
    {
        GivenActiveEnablement();
        GivenCatalog(isSuccess: false, Modality("m-1", "1", ESuretyBranch.Public));

        var summary = await BuildImporter().RunAsync(Now, CancellationToken.None);

        summary.InsurersFailed.Should().Be(1);
        summary.InsurersSucceeded.Should().Be(0);
        await _modalities.DidNotReceive().AddAsync(Arg.Any<ImportedModality>(), Arg.Any<CancellationToken>());
        await _modalities.DidNotReceive().ListActiveByInsurerAsync(InsurerId, Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-038")]
    public async Task Run_DeveDesativar_QuandoImportadaSumiuNaImportacaoBemSucedida()
    {
        GivenActiveEnablement();
        GivenCatalog(isSuccess: true, Modality("m-1", "1", ESuretyBranch.Public));
        _groups.GetByInsurerAndSourceAsync(InsurerId, "g-1", Arg.Any<CancellationToken>()).Returns((ImportedGroup?)null);
        _modalities.GetByInsurerAndSourceAsync(InsurerId, "m-1", Arg.Any<CancellationToken>()).Returns((ImportedModality?)null);
        _catalog.GetByGlobalExternalIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Modality?)null);

        var vanished = ImportedModality.Create(
            InsurerId, "m-old", "Antiga", ESuretyBranch.Public, "9", "X", null, null, Now);
        _modalities.ListActiveByInsurerAsync(InsurerId, Arg.Any<CancellationToken>())
            .Returns(new List<ImportedModality> { vanished });

        await BuildImporter().RunAsync(Now, CancellationToken.None);

        vanished.Status.Should().Be(EImportedModalityStatus.Inactive);
    }
}
