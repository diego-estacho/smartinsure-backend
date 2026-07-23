using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.GetModality;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.GetModality.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ListModalities;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.ListModalities.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.UpdateModality;
using SmartInsure.Application.UseCase.UseCases.ModalityUseCases.UpdateModality.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.ModalityUseCases;

/// <summary>RN-032 — Edição de Modalidade (nome/descrição): nome único.</summary>
[Trait("RuleId", "RN-032")]
public class UpdateModalityUseCaseTests
{
    private readonly IModalityRepository _modalityRepository = Substitute.For<IModalityRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateModalityUseCase _useCase;

    public UpdateModalityUseCaseTests()
        => _useCase = new UpdateModalityUseCase(_modalityRepository, _unitOfWork);

    private static Modality Existing()
        => Modality.CreateManual("Garantia Judicial", null, EModalityStatus.Active);

    [Fact]
    public async Task Execute_DeveRecusar_QuandoModalidadeNaoEncontrada()
    {
        var id = Guid.CreateVersion7();
        _modalityRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Modality?)null);

        var act = () => _useCase.ExecuteAsync(
            new UpdateModalityRequest(id, "Nova", null), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoNomeJaExisteEmOutraModalidade()
    {
        var modality = Existing();
        _modalityRepository.GetByIdAsync(modality.Id, Arg.Any<CancellationToken>()).Returns(modality);
        _modalityRepository.NameExistsAsync("Garantia de Execução", modality.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        var act = () => _useCase.ExecuteAsync(
            new UpdateModalityRequest(modality.Id, "Garantia de Execução", null),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Execute_DeveEditar_QuandoDadosValidos()
    {
        var modality = Existing();
        _modalityRepository.GetByIdAsync(modality.Id, Arg.Any<CancellationToken>()).Returns(modality);
        _modalityRepository.NameExistsAsync(Arg.Any<string>(), modality.Id, Arg.Any<CancellationToken>()).Returns(false);

        var response = await _useCase.ExecuteAsync(
            new UpdateModalityRequest(modality.Id, "Garantia Recursal", "Recursos"), CancellationToken.None);

        response.Name.Should().Be("Garantia Recursal");
        response.Description.Should().Be("Recursos");
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}

/// <summary>RN-032 — Detalhe de Modalidade.</summary>
[Trait("RuleId", "RN-032")]
public class GetModalityUseCaseTests
{
    private readonly IModalityRepository _repository = Substitute.For<IModalityRepository>();
    private readonly GetModalityUseCase _useCase;

    public GetModalityUseCaseTests() => _useCase = new GetModalityUseCase(_repository);

    [Fact]
    public async Task Execute_DeveRetornarDetalhe_QuandoEncontrada()
    {
        var modality = Modality.CreateManual("Garantia Judicial", "Desc", EModalityStatus.Active);
        _repository.GetByIdAsync(modality.Id, Arg.Any<CancellationToken>()).Returns(modality);

        var response = await _useCase.ExecuteAsync(new GetModalityRequest(modality.Id), CancellationToken.None);

        response.Name.Should().Be("Garantia Judicial");
        response.Status.Should().Be("Active");
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoNaoEncontrada()
    {
        var id = Guid.CreateVersion7();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Modality?)null);

        var act = () => _useCase.ExecuteAsync(new GetModalityRequest(id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

/// <summary>RN-036/RN-039 — Listagem de Modalidades: inativas só para o Administrador.</summary>
[Trait("RuleId", "RN-036")]
public class ListModalitiesUseCaseTests
{
    private readonly IModalityRepository _repository = Substitute.For<IModalityRepository>();
    private readonly ListModalitiesUseCase _useCase;

    public ListModalitiesUseCaseTests() => _useCase = new ListModalitiesUseCase(_repository);

    [Fact]
    public async Task Execute_DeveIncluirInativas_QuandoChamadorEhAdministrador()
    {
        _repository.ListAsync(1, 20, true, Arg.Any<CancellationToken>())
            .Returns((new List<ModalityListItemDto>(), 0L));

        await _useCase.ExecuteAsync(
            new ListModalitiesRequest { IncludeInactive = true, CallerIsSystemAdministrator = true },
            CancellationToken.None);

        await _repository.Received(1).ListAsync(1, 20, true, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_NaoDeveIncluirInativas_QuandoChamadorNaoEhAdministrador()
    {
        _repository.ListAsync(1, 20, false, Arg.Any<CancellationToken>())
            .Returns((new List<ModalityListItemDto>(), 0L));

        await _useCase.ExecuteAsync(
            new ListModalitiesRequest { IncludeInactive = true, CallerIsSystemAdministrator = false },
            CancellationToken.None);

        await _repository.Received(1).ListAsync(1, 20, false, Arg.Any<CancellationToken>());
    }
}
