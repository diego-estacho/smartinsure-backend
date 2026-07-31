using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotationBook;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotationBook.Requests;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.ListQuotationBook.Responses;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using Xunit;

namespace SmartInsure.Tests.Application.UseCases.QuotationUseCases.ListQuotationBook;

[Trait("Category", "UseCase")]
public sealed class ListQuotationBookUseCaseTests
{
    private readonly IQuotationRepository _quotationRepository = Substitute.For<IQuotationRepository>();
    private readonly IInsurerRepository _insurerRepository = Substitute.For<IInsurerRepository>();
    private readonly ListQuotationBookUseCase _useCase;

    private readonly Guid _brokerageId = Guid.NewGuid();

    public ListQuotationBookUseCaseTests()
        => _useCase = new ListQuotationBookUseCase(_quotationRepository, _insurerRepository);

    private static QuotationBookItemDto Item(Guid insurerId, EQuotationResult result = EQuotationResult.ReadyForEmission)
        => new(
            QuotationId: Guid.NewGuid(),
            Number: "PROP-1",
            PolicyHolderName: "Pilão Engenharia Ltda",
            InsuredName: "Secretaria Municipal",
            InsurerId: insurerId,
            ModalityName: "Executante Fornecedor",
            InsuredAmount: 1_500_000m,
            Premium: 18_000m,
            CommissionPercentage: 20m,
            Result: result,
            CoverageStartDate: new DateOnly(2026, 7, 29),
            CoverageEndDate: new DateOnly(2027, 7, 29),
            CreatedAt: DateTime.UtcNow);

    private void ArrangeBook(QuotationBookPageDto page)
        => _quotationRepository.ListBookAsync(
                _brokerageId, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(),
                Arg.Any<EQuotationResult?>(), Arg.Any<CancellationToken>())
            .Returns(page);

    [Fact]
    [Trait("RuleId", "RN-077")]
    public async Task Execute_DeveRecusar_QuandoSemCorretoraAtiva()
    {
        var act = async () => await _useCase.ExecuteAsync(
            new ListQuotationBookRequest { ActiveBrokerageId = null }, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
        await _quotationRepository.DidNotReceiveWithAnyArgs()
            .ListBookAsync(default, default, default, default, default, default);
    }

    [Fact]
    public async Task Execute_DeveSanearPaginacao()
    {
        ArrangeBook(new QuotationBookPageDto([], 0, []));

        var result = await _useCase.ExecuteAsync(
            new ListQuotationBookRequest { ActiveBrokerageId = _brokerageId, Page = 0, PageSize = 500 },
            CancellationToken.None);

        result.Page.Should().Be(1);
        result.PageSize.Should().Be(100);
        await _quotationRepository.Received(1).ListBookAsync(
            _brokerageId, 1, 100, null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-078")]
    public async Task Execute_DeveFiltrarPorSituacao_PeloNomeEstavel()
    {
        ArrangeBook(new QuotationBookPageDto([], 0, []));

        await _useCase.ExecuteAsync(
            new ListQuotationBookRequest { ActiveBrokerageId = _brokerageId, Situation = "analysis" },
            CancellationToken.None);

        await _quotationRepository.Received(1).ListBookAsync(
            _brokerageId, 1, 20, null, EQuotationResult.Analysis, Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-078")]
    public async Task Execute_DeveRecusarSituacaoInvalida()
    {
        var act = async () => await _useCase.ExecuteAsync(
            new ListQuotationBookRequest { ActiveBrokerageId = _brokerageId, Situation = "Arquivada" },
            CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        await _quotationRepository.DidNotReceiveWithAnyArgs()
            .ListBookAsync(default, default, default, default, default, default);
    }

    [Fact]
    [Trait("RuleId", "RN-077")]
    public async Task Execute_DeveResolverNomeELogoDaSeguradora_ComFallback()
    {
        var comLogo = Guid.NewGuid();
        var semCadastro = Guid.NewGuid();
        ArrangeBook(new QuotationBookPageDto([Item(comLogo), Item(semCadastro)], 2, []));
        _insurerRepository.GetCorporateNamesByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, string> { [comLogo] = "Newe Seguros" });
        _insurerRepository.GetLogoUrlsByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, string> { [comLogo] = "https://cdn/newe.png" });

        var result = await _useCase.ExecuteAsync(
            new ListQuotationBookRequest { ActiveBrokerageId = _brokerageId }, CancellationToken.None);

        result.Items[0].InsurerName.Should().Be("Newe Seguros");
        result.Items[0].InsurerLogoUrl.Should().Be("https://cdn/newe.png");
        result.Items[1].InsurerName.Should().Be("Seguradora");
        result.Items[1].InsurerLogoUrl.Should().BeNull();
        result.Items[0].Result.Should().Be("ReadyForEmission");
    }

    [Fact]
    [Trait("RuleId", "RN-078")]
    public async Task Execute_DeveMapearContagemPorSituacao_PeloNomeEstavel()
    {
        ArrangeBook(new QuotationBookPageDto(
            [],
            0,
            [
                new QuotationSituationCountDto(EQuotationResult.ReadyForEmission, 3),
                new QuotationSituationCountDto(EQuotationResult.Analysis, 1),
            ]));

        var result = await _useCase.ExecuteAsync(
            new ListQuotationBookRequest { ActiveBrokerageId = _brokerageId }, CancellationToken.None);

        result.Counts.Should().BeEquivalentTo(new[]
        {
            new QuotationSituationCountResponse("ReadyForEmission", 3),
            new QuotationSituationCountResponse("Analysis", 1),
        });
    }

    [Fact]
    public async Task Execute_DeveRepassarBuscaEPaginacao()
    {
        ArrangeBook(new QuotationBookPageDto([], 0, []));

        await _useCase.ExecuteAsync(
            new ListQuotationBookRequest
            {
                ActiveBrokerageId = _brokerageId,
                Page = 2,
                PageSize = 10,
                Search = "pilão",
            },
            CancellationToken.None);

        await _quotationRepository.Received(1).ListBookAsync(
            _brokerageId, 2, 10, "pilão", null, Arg.Any<CancellationToken>());
    }
}
