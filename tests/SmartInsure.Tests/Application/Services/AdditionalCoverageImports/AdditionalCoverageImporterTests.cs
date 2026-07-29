using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SmartInsure.Application.UseCase.Services.AdditionalCoverageImports;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;

namespace SmartInsure.Tests.Application.Services.AdditionalCoverageImports;

/// <summary>RN-041/RN-042/RN-044/RN-045 — importação de Coberturas Adicionais Importadas pelo Motor de Cálculo.</summary>
public class AdditionalCoverageImporterTests
{
    private static readonly DateTime Now = new(2026, 7, 23, 5, 0, 0, DateTimeKind.Utc);
    private static readonly Guid InsurerId = Guid.CreateVersion7();
    private static readonly Guid ModalityId = Guid.CreateVersion7();

    private readonly IBrokerageInsurerEnablementRepository _enablements =
        Substitute.For<IBrokerageInsurerEnablementRepository>();

    private readonly IImportedModalityRepository _modalities = Substitute.For<IImportedModalityRepository>();
    private readonly IImportedAdditionalCoverageRepository _coverages =
        Substitute.For<IImportedAdditionalCoverageRepository>();
    private readonly ICalculationEngine _engine = Substitute.For<ICalculationEngine>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private AdditionalCoverageImporter BuildImporter()
    {
        var provider = new ServiceCollection()
            .AddKeyedSingleton(ECalculationEngine.PlugV2, _engine)
            .BuildServiceProvider();

        return new AdditionalCoverageImporter(_enablements, _modalities, _coverages, provider, _unitOfWork);
    }

    private void GivenActiveEnablement()
        => _enablements.ListActiveForImportAsync(Arg.Any<CancellationToken>()).Returns(
            new List<ActiveEnablementImportDto>
            {
                new(Guid.CreateVersion7(), Guid.CreateVersion7(), "34060267000196",
                    InsurerId, "ins-uid", "PlugV2", "{\"baseUrl\":\"https://x\",\"key\":\"k\"}"),
            });

    private void GivenImportableModality(ESuretyBranch branch)
        => _modalities.ListImportableForCoverageAsync(InsurerId, Arg.Any<CancellationToken>()).Returns(
            new List<ImportableModalityForCoverageDto>
            {
                new(ModalityId, "Executante Construtor", "GARANTIA_TRADICIONAL", branch),
            });

    private void GivenCoverages(bool isSuccess, params ImportedAdditionalCoverageData[] coverages)
        => _engine.GetAdditionalCoveragesAsync(
                Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new ImportedAdditionalCoverageResult(isSuccess, coverages, isSuccess ? null : "falha na origem"));

    private static ImportedAdditionalCoverageData Coverage(string name, ESuretyBranch branch)
        => new(name, "uid-" + name.Trim(), 1, true, branch);

    [Fact]
    [Trait("RuleId", "RN-041")]
    public async Task Run_DeveCriarImportada_QuandoConsultaBemSucedida()
    {
        GivenActiveEnablement();
        GivenImportableModality(ESuretyBranch.Private);
        GivenCoverages(isSuccess: true, Coverage("Multa", ESuretyBranch.Private));
        _coverages.GetByImportedModalityAndNameAsync(ModalityId, "Multa", Arg.Any<CancellationToken>())
            .Returns((ImportedAdditionalCoverage?)null);
        _coverages.ListActiveByImportedModalityAsync(ModalityId, Arg.Any<CancellationToken>())
            .Returns(new List<ImportedAdditionalCoverage>());

        var summary = await BuildImporter().RunAsync(Now, CancellationToken.None);

        summary.ModalitiesSucceeded.Should().Be(1);
        await _coverages.Received(1).AddAsync(
            Arg.Is<ImportedAdditionalCoverage>(c =>
                c.Name == "Multa"
                && c.Status == EImportedAdditionalCoverageStatus.Active
                && c.AdditionalCoverageId == null),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-043")]
    public async Task Run_DeveReativarEPreservarVinculo_QuandoImportadaJaConhecida()
    {
        GivenActiveEnablement();
        GivenImportableModality(ESuretyBranch.Private);
        GivenCoverages(isSuccess: true, Coverage("Multa", ESuretyBranch.Private));

        var existing = ImportedAdditionalCoverage.Create(ModalityId, "Multa", "old", 9, false, Now);
        var canonical = Guid.CreateVersion7();
        existing.LinkTo(canonical);
        existing.Deactivate();
        _coverages.GetByImportedModalityAndNameAsync(ModalityId, "Multa", Arg.Any<CancellationToken>())
            .Returns(existing);
        _coverages.ListActiveByImportedModalityAsync(ModalityId, Arg.Any<CancellationToken>())
            .Returns(new List<ImportedAdditionalCoverage>());

        await BuildImporter().RunAsync(Now, CancellationToken.None);

        await _coverages.DidNotReceive().AddAsync(Arg.Any<ImportedAdditionalCoverage>(), Arg.Any<CancellationToken>());
        existing.Status.Should().Be(EImportedAdditionalCoverageStatus.Active);
        // RN-043: o vínculo manual é preservado.
        existing.AdditionalCoverageId.Should().Be(canonical);
    }

    [Fact]
    [Trait("RuleId", "RN-041")]
    public async Task Run_DeveIgnorarCobertura_QuandoSemNome()
    {
        GivenActiveEnablement();
        GivenImportableModality(ESuretyBranch.Private);
        GivenCoverages(isSuccess: true, Coverage("   ", ESuretyBranch.Private));
        _coverages.ListActiveByImportedModalityAsync(ModalityId, Arg.Any<CancellationToken>())
            .Returns(new List<ImportedAdditionalCoverage>());

        var summary = await BuildImporter().RunAsync(Now, CancellationToken.None);

        summary.ModalitiesSucceeded.Should().Be(1);
        await _coverages.DidNotReceive().AddAsync(Arg.Any<ImportedAdditionalCoverage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-041")]
    public async Task Run_DeveDescartarCobertura_QuandoRamoDivergente()
    {
        GivenActiveEnablement();
        GivenImportableModality(ESuretyBranch.Public);
        GivenCoverages(isSuccess: true, Coverage("Multa", ESuretyBranch.Private));
        _coverages.ListActiveByImportedModalityAsync(ModalityId, Arg.Any<CancellationToken>())
            .Returns(new List<ImportedAdditionalCoverage>());

        await BuildImporter().RunAsync(Now, CancellationToken.None);

        await _coverages.DidNotReceive().AddAsync(Arg.Any<ImportedAdditionalCoverage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-045")]
    public async Task Run_NaoDeveDesativarNemUpsert_QuandoModalidadeFalha()
    {
        GivenActiveEnablement();
        GivenImportableModality(ESuretyBranch.Private);
        GivenCoverages(isSuccess: false, Coverage("Multa", ESuretyBranch.Private));

        var summary = await BuildImporter().RunAsync(Now, CancellationToken.None);

        summary.ModalitiesFailed.Should().Be(1);
        summary.ModalitiesSucceeded.Should().Be(0);
        await _coverages.DidNotReceive().AddAsync(Arg.Any<ImportedAdditionalCoverage>(), Arg.Any<CancellationToken>());
        await _coverages.DidNotReceive().ListActiveByImportedModalityAsync(ModalityId, Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-044")]
    public async Task Run_DeveDesativarCobertura_QuandoSumiuNaConsultaBemSucedida()
    {
        GivenActiveEnablement();
        GivenImportableModality(ESuretyBranch.Private);
        GivenCoverages(isSuccess: true, Coverage("Multa", ESuretyBranch.Private));
        _coverages.GetByImportedModalityAndNameAsync(ModalityId, "Multa", Arg.Any<CancellationToken>())
            .Returns((ImportedAdditionalCoverage?)null);

        var vanished = ImportedAdditionalCoverage.Create(ModalityId, "Trabalhista", "uid-t", 1, false, Now);
        _coverages.ListActiveByImportedModalityAsync(ModalityId, Arg.Any<CancellationToken>())
            .Returns(new List<ImportedAdditionalCoverage> { vanished });

        await BuildImporter().RunAsync(Now, CancellationToken.None);

        vanished.Status.Should().Be(EImportedAdditionalCoverageStatus.Inactive);
    }
}
