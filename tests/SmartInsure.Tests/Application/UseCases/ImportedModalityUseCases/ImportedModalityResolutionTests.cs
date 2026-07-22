using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.IgnoreImportedModality;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.IgnoreImportedModality.Requests;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.ReassignImportedModality;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.ReassignImportedModality.Requests;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.RestoreImportedModality;
using SmartInsure.Application.UseCase.UseCases.ImportedModalityUseCases.RestoreImportedModality.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.ImportedModalityUseCases;

/// <summary>RN-034 — curadoria da Fila: reatribuir (override Manual), ignorar e reativar.</summary>
public class ImportedModalityResolutionTests
{
    private static readonly DateTime Now = new(2026, 7, 22, 3, 0, 0, DateTimeKind.Utc);

    private readonly IImportedModalityRepository _imported = Substitute.For<IImportedModalityRepository>();
    private readonly IModalityRepository _modalities = Substitute.For<IModalityRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private ReassignImportedModalityUseCase ReassignUseCase()
        => new(_imported, _modalities, _unitOfWork);

    private static ImportedModality Imported(ESuretyBranch branch = ESuretyBranch.Public)
        => ImportedModality.Create(
            Guid.CreateVersion7(), "m-1", "Garantia", branch, "1", "Execução", null, null, Now);

    private static Modality Modality()
        => SmartInsure.Core.Entities.Modality.CreateManual("Garantia de Execução", null, EModalityStatus.Active);

    [Fact]
    [Trait("RuleId", "RN-034")]
    public async Task Reassign_DeveVincularComoManual_QuandoValido()
    {
        var imported = Imported();
        var modality = Modality();
        _imported.GetByIdAsync(imported.Id, Arg.Any<CancellationToken>()).Returns(imported);
        _modalities.GetByIdAsync(modality.Id, Arg.Any<CancellationToken>()).Returns(modality);

        var response = await ReassignUseCase().ExecuteAsync(
            new ReassignImportedModalityRequest(imported.Id, modality.Id), CancellationToken.None);

        response.LinkSource.Should().Be("Manual");
        response.ModalityId.Should().Be(modality.Id);
        imported.ModalityId.Should().Be(modality.Id);
        imported.ModalityLinkSource.Should().Be(EModalityLinkSource.Manual);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-034")]
    public async Task Reassign_DeveRecusar_QuandoModalidadeDestinoNaoExiste()
    {
        var imported = Imported();
        _imported.GetByIdAsync(imported.Id, Arg.Any<CancellationToken>()).Returns(imported);
        var modalityId = Guid.CreateVersion7();
        _modalities.GetByIdAsync(modalityId, Arg.Any<CancellationToken>()).Returns((Modality?)null);

        var act = () => ReassignUseCase().ExecuteAsync(
            new ReassignImportedModalityRequest(imported.Id, modalityId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-034")]
    public async Task Reassign_DeveRecusar_QuandoImportadaNaoEncontrada()
    {
        var id = Guid.CreateVersion7();
        _imported.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((ImportedModality?)null);

        var act = () => ReassignUseCase().ExecuteAsync(
            new ReassignImportedModalityRequest(id, Guid.CreateVersion7()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
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

    [Fact]
    [Trait("RuleId", "RN-034")]
    public async Task Restore_DeveDesfazerIgnorar()
    {
        var imported = Imported();
        imported.Ignore();
        _imported.GetByIdAsync(imported.Id, Arg.Any<CancellationToken>()).Returns(imported);

        var useCase = new RestoreImportedModalityUseCase(_imported, _unitOfWork);
        var response = await useCase.ExecuteAsync(
            new RestoreImportedModalityRequest(imported.Id), CancellationToken.None);

        response.Ignored.Should().BeFalse();
        imported.IsIgnored.Should().BeFalse();
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}
