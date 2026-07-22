using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.IgnoreImportedModality;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.IgnoreImportedModality.Requests;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.MapImportedModality;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.MapImportedModality.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.ImportedModalityUseCases;

/// <summary>RN-034 — resolução da Fila: mapear (manual) e ignorar; trava de ramo (RN-032).</summary>
public class ImportedModalityResolutionTests
{
    private static readonly DateTime Now = new(2026, 7, 22, 3, 0, 0, DateTimeKind.Utc);

    private readonly IImportedModalityRepository _imported = Substitute.For<IImportedModalityRepository>();
    private readonly IModalityRepository _modalities = Substitute.For<IModalityRepository>();
    private readonly IModalityMappingRepository _mappings = Substitute.For<IModalityMappingRepository>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private MapImportedModalityUseCase MapUseCase()
        => new(_imported, _modalities, _mappings, _currentUser, _unitOfWork);

    private static ImportedModality Imported(ESuretyBranch branch = ESuretyBranch.Public)
        => ImportedModality.Create(
            Guid.CreateVersion7(), "m-1", "Garantia", branch, "1", "Execução", null, null, Now);

    private static Modality Modality()
        => SmartInsure.Core.Entities.Modality.Create(
            "Garantia de Execução", Guid.CreateVersion7(), null, EModalityStatus.Active);

    [Fact]
    [Trait("RuleId", "RN-034")]
    public async Task Map_DeveConfirmarManual_QuandoValido()
    {
        var imported = Imported();
        var modality = Modality();
        _imported.GetByIdAsync(imported.Id, Arg.Any<CancellationToken>()).Returns(imported);
        _modalities.GetByIdAsync(modality.Id, Arg.Any<CancellationToken>()).Returns(modality);
        _mappings.HasConfirmedAsync(imported.Id, Arg.Any<CancellationToken>()).Returns(false);
        _imported.HasConfirmedBranchConflictAsync(modality.Id, ESuretyBranch.Public, Arg.Any<CancellationToken>()).Returns(false);
        _currentUser.UserIdentifier.Returns("admin@dev");

        var response = await MapUseCase().ExecuteAsync(
            new MapImportedModalityRequest(imported.Id, modality.Id), CancellationToken.None);

        response.MappingStatus.Should().Be("Confirmed");
        await _mappings.Received(1).AddAsync(
            Arg.Is<ModalityMapping>(m => m.Establishment == EMappingEstablishment.Manual
                && m.Status == EModalityMappingStatus.Confirmed
                && m.ConfirmedBy == "admin@dev"),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-034")]
    public async Task Map_DeveRecusar_QuandoRamoIncompativel()
    {
        var imported = Imported(ESuretyBranch.Public);
        var modality = Modality();
        _imported.GetByIdAsync(imported.Id, Arg.Any<CancellationToken>()).Returns(imported);
        _modalities.GetByIdAsync(modality.Id, Arg.Any<CancellationToken>()).Returns(modality);
        _mappings.HasConfirmedAsync(imported.Id, Arg.Any<CancellationToken>()).Returns(false);
        _imported.HasConfirmedBranchConflictAsync(modality.Id, ESuretyBranch.Public, Arg.Any<CancellationToken>()).Returns(true);

        var act = () => MapUseCase().ExecuteAsync(
            new MapImportedModalityRequest(imported.Id, modality.Id), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        await _mappings.DidNotReceive().AddAsync(Arg.Any<ModalityMapping>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-034")]
    public async Task Map_DeveRecusar_QuandoJaMapeada()
    {
        var imported = Imported();
        var modality = Modality();
        _imported.GetByIdAsync(imported.Id, Arg.Any<CancellationToken>()).Returns(imported);
        _modalities.GetByIdAsync(modality.Id, Arg.Any<CancellationToken>()).Returns(modality);
        _mappings.HasConfirmedAsync(imported.Id, Arg.Any<CancellationToken>()).Returns(true);

        var act = () => MapUseCase().ExecuteAsync(
            new MapImportedModalityRequest(imported.Id, modality.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    [Trait("RuleId", "RN-034")]
    public async Task Ignore_DeveMarcarIgnorada()
    {
        var imported = Imported();
        _imported.GetByIdAsync(imported.Id, Arg.Any<CancellationToken>()).Returns(imported);

        var useCase = new IgnoreImportedModalityUseCase(_imported, _unitOfWork);
        var response = await useCase.ExecuteAsync(
            new IgnoreImportedModalityRequest(imported.Id), CancellationToken.None);

        response.Ignored.Should().BeTrue();
        imported.IsIgnored.Should().BeTrue();
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-034")]
    public async Task Ignore_DeveRecusar_QuandoNaoEncontrada()
    {
        var id = Guid.CreateVersion7();
        _imported.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((ImportedModality?)null);

        var useCase = new IgnoreImportedModalityUseCase(_imported, _unitOfWork);
        var act = () => useCase.ExecuteAsync(new IgnoreImportedModalityRequest(id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
