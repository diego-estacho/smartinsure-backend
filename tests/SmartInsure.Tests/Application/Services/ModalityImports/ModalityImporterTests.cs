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

/// <summary>RN-030/RN-031/RN-032/RN-035 — importação de modalidades pelo Motor de Cálculo.</summary>
public class ModalityImporterTests
{
    private static readonly DateTime Now = new(2026, 7, 22, 3, 0, 0, DateTimeKind.Utc);
    private static readonly Guid InsurerId = Guid.CreateVersion7();

    private readonly IBrokerageInsurerEnablementRepository _enablements =
        Substitute.For<IBrokerageInsurerEnablementRepository>();

    private readonly IImportedGroupRepository _groups = Substitute.For<IImportedGroupRepository>();
    private readonly IImportedModalityRepository _modalities = Substitute.For<IImportedModalityRepository>();
    private readonly IModalityMappingRepository _mappings = Substitute.For<IModalityMappingRepository>();
    private readonly ICalculationEngine _engine = Substitute.For<ICalculationEngine>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private ModalityImporter BuildImporter()
    {
        var provider = new ServiceCollection()
            .AddKeyedSingleton(ECalculationEngine.PlugV2, _engine)
            .BuildServiceProvider();

        return new ModalityImporter(_enablements, _groups, _modalities, _mappings, provider, _unitOfWork);
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
    [Trait("RuleId", "RN-030")]
    public async Task Run_DeveCriarImportadaEMapearPorIdentificador_QuandoSucesso()
    {
        GivenActiveEnablement();
        GivenCatalog(isSuccess: true, Modality("m-1", "1", ESuretyBranch.Public));
        _groups.GetByInsurerAndSourceAsync(InsurerId, "g-1", Arg.Any<CancellationToken>()).Returns((ImportedGroup?)null);
        _modalities.GetByInsurerAndSourceAsync(InsurerId, "m-1", Arg.Any<CancellationToken>()).Returns((ImportedModality?)null);
        _mappings.HasConfirmedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var modalityId = Guid.CreateVersion7();
        _modalities.FindConfirmedModalityIdByEngineAsync("1", ESuretyBranch.Public, Arg.Any<CancellationToken>())
            .Returns(modalityId);
        _modalities.ListActiveByInsurerAsync(InsurerId, Arg.Any<CancellationToken>())
            .Returns(new List<ImportedModality>());

        var summary = await BuildImporter().RunAsync(Now, CancellationToken.None);

        summary.InsurersSucceeded.Should().Be(1);
        await _groups.Received(1).AddAsync(Arg.Any<ImportedGroup>(), Arg.Any<CancellationToken>());
        await _modalities.Received(1).AddAsync(Arg.Any<ImportedModality>(), Arg.Any<CancellationToken>());
        await _mappings.Received(1).AddAsync(
            Arg.Is<ModalityMapping>(m => m.ModalityId == modalityId
                && m.Establishment == EMappingEstablishment.Identifier
                && m.Status == EModalityMappingStatus.Confirmed),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-032")]
    public async Task Run_NaoDeveMapear_QuandoIdentificadorSemModalidade()
    {
        GivenActiveEnablement();
        GivenCatalog(isSuccess: true, Modality("m-1", "1", ESuretyBranch.Public));
        _groups.GetByInsurerAndSourceAsync(InsurerId, "g-1", Arg.Any<CancellationToken>()).Returns((ImportedGroup?)null);
        _modalities.GetByInsurerAndSourceAsync(InsurerId, "m-1", Arg.Any<CancellationToken>()).Returns((ImportedModality?)null);
        _mappings.HasConfirmedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        _modalities.FindConfirmedModalityIdByEngineAsync("1", ESuretyBranch.Public, Arg.Any<CancellationToken>())
            .Returns((Guid?)null);
        _modalities.ListActiveByInsurerAsync(InsurerId, Arg.Any<CancellationToken>())
            .Returns(new List<ImportedModality>());

        await BuildImporter().RunAsync(Now, CancellationToken.None);

        await _mappings.DidNotReceive().AddAsync(Arg.Any<ModalityMapping>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-035")]
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
    [Trait("RuleId", "RN-035")]
    public async Task Run_DeveDesativar_QuandoImportadaSumiuNaImportacaoBemSucedida()
    {
        GivenActiveEnablement();
        GivenCatalog(isSuccess: true, Modality("m-1", "1", ESuretyBranch.Public));
        _groups.GetByInsurerAndSourceAsync(InsurerId, "g-1", Arg.Any<CancellationToken>()).Returns((ImportedGroup?)null);
        _modalities.GetByInsurerAndSourceAsync(InsurerId, "m-1", Arg.Any<CancellationToken>()).Returns((ImportedModality?)null);
        _mappings.HasConfirmedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        _modalities.FindConfirmedModalityIdByEngineAsync(Arg.Any<string>(), Arg.Any<ESuretyBranch>(), Arg.Any<CancellationToken>())
            .Returns((Guid?)null);

        var vanished = ImportedModality.Create(
            InsurerId, "m-old", "Antiga", ESuretyBranch.Public, "9", "X", null, null, Now);
        _modalities.ListActiveByInsurerAsync(InsurerId, Arg.Any<CancellationToken>())
            .Returns(new List<ImportedModality> { vanished });

        await BuildImporter().RunAsync(Now, CancellationToken.None);

        vanished.Status.Should().Be(EImportedModalityStatus.Inactive);
    }
}
