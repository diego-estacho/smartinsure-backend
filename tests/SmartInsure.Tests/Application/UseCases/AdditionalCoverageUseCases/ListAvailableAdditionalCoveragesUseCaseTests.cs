using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.ListAvailableAdditionalCoverages;
using SmartInsure.Application.UseCase.UseCases.AdditionalCoverageUseCases.ListAvailableAdditionalCoverages.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.AdditionalCoverageUseCases;

/// <summary>
/// RN-104/RN-046 — Coberturas Adicionais ofertáveis na etapa de risco (união simples, derivada dos
/// vínculos ativos, escopada pela Corretora ativa — RN-103).
/// </summary>
[Trait("RuleId", "RN-104")]
public sealed class ListAvailableAdditionalCoveragesUseCaseTests
{
    private readonly IImportedAdditionalCoverageRepository _repository =
        Substitute.For<IImportedAdditionalCoverageRepository>();

    private readonly IModalityRepository _modalityRepository = Substitute.For<IModalityRepository>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly ListAvailableAdditionalCoveragesUseCase _useCase;

    private static readonly Guid BrokerageId = Guid.CreateVersion7();
    private static readonly Guid ModalityId = Guid.CreateVersion7();

    public ListAvailableAdditionalCoveragesUseCaseTests()
        => _useCase = new ListAvailableAdditionalCoveragesUseCase(
            _repository, _modalityRepository, _currentUser);

    [Fact]
    public async Task Execute_DeveListarAsCanonicasOfertaveis_RN104()
    {
        var multa = Guid.CreateVersion7();
        var trabalhista = Guid.CreateVersion7();
        SetupModality();
        _currentUser.ActiveBrokerageId.Returns(BrokerageId);
        _repository.ListAvailableForModalityAsync(BrokerageId, ModalityId, Arg.Any<CancellationToken>())
            .Returns(
            [
                new AvailableAdditionalCoverageDto(trabalhista, "Trabalhista e Previdenciária"),
                new AvailableAdditionalCoverageDto(multa, "Multas"),
            ]);

        var response = await _useCase.ExecuteAsync(
            new ListAvailableAdditionalCoveragesRequest(ModalityId), CancellationToken.None);

        response.Items.Select(item => item.Id).Should().BeEquivalentTo(new[] { multa, trabalhista });
        // Ordenado por nome, para a etapa 3 apresentar de forma estável.
        response.Items.Select(item => item.Name).Should()
            .ContainInOrder("Multas", "Trabalhista e Previdenciária");
    }

    [Fact]
    public async Task Execute_DeveEscoparPelaCorretoraAtiva_RN103()
    {
        SetupModality();
        _currentUser.ActiveBrokerageId.Returns(BrokerageId);
        _repository.ListAvailableForModalityAsync(
                Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns([]);

        await _useCase.ExecuteAsync(
            new ListAvailableAdditionalCoveragesRequest(ModalityId), CancellationToken.None);

        await _repository.Received(1).ListAvailableForModalityAsync(
            BrokerageId, ModalityId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveDevolverVazio_QuandoModalidadeNaoTemCobertura_RN104()
    {
        SetupModality();
        _currentUser.ActiveBrokerageId.Returns(BrokerageId);
        _repository.ListAvailableForModalityAsync(
                Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var response = await _useCase.ExecuteAsync(
            new ListAvailableAdditionalCoveragesRequest(ModalityId), CancellationToken.None);

        response.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Execute_DeveDevolverVazio_QuandoNaoHaCorretoraAtiva_RN103()
    {
        // ADR-065: Escopo ausente é estado legítimo, não violação de regra. A oferta é derivada das
        // Seguradoras habilitadas da Corretora ativa, então sem Corretora ativa ela é vazia — recusar
        // quebraria a renderização da etapa de risco. Quem recusa por falta de Escopo é o cotar.
        SetupModality();
        _currentUser.ActiveBrokerageId.Returns((Guid?)null);

        var response = await _useCase.ExecuteAsync(
            new ListAvailableAdditionalCoveragesRequest(ModalityId), CancellationToken.None);

        response.Items.Should().BeEmpty();
        await _repository.DidNotReceiveWithAnyArgs()
            .ListAvailableForModalityAsync(default, default, default);
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoModalidadeNaoExiste_RN104()
    {
        _currentUser.ActiveBrokerageId.Returns(BrokerageId);
        _modalityRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Modality?)null);

        var act = () => _useCase.ExecuteAsync(
            new ListAvailableAdditionalCoveragesRequest(ModalityId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    private void SetupModality()
        => _modalityRepository.GetByIdAsync(ModalityId, Arg.Any<CancellationToken>())
            .Returns(Modality.CreateManual("Licitante", null, EModalityStatus.Active));
}
