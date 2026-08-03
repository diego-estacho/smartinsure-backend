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
    private readonly ListQuotationBookUseCase _useCase;

    private readonly Guid _brokerageId = Guid.NewGuid();

    public ListQuotationBookUseCaseTests()
        => _useCase = new ListQuotationBookUseCase(_quotationRepository);

    private static QuotationBookItemDto Item(
        EQuotationResult result = EQuotationResult.ReadyForEmission, bool requiresCcg = false)
        => new(
            QuotationId: Guid.NewGuid(),
            Number: "PROP-1",
            PolicyHolderName: "Pilão Engenharia Ltda",
            InsuredName: "Secretaria Municipal",
            InsurerId: Guid.NewGuid(),
            InsurerName: "Newe Seguros",
            InsurerLogoUrl: "https://cdn/newe.png",
            ModalityId: Guid.NewGuid(),
            ModalityName: "Executante Fornecedor",
            InsuredAmount: 1_500_000m,
            Premium: 18_000m,
            CommissionPercentage: 20m,
            Result: result,
            RequiresCcg: requiresCcg,
            CoverageStartDate: new DateOnly(2026, 7, 29),
            CoverageEndDate: new DateOnly(2027, 7, 29),
            CreatedAt: DateTime.UtcNow);

    private static QuotationBookPageDto Page(
        IReadOnlyList<QuotationBookItemDto>? items = null,
        IReadOnlyList<QuotationSituationCountDto>? counts = null,
        IReadOnlyList<QuotationBookOptionDto>? insurers = null,
        IReadOnlyList<QuotationBookOptionDto>? modalities = null)
        => new(items ?? [], items?.Count ?? 0, counts ?? [], insurers ?? [], modalities ?? []);

    private void ArrangeBook(QuotationBookPageDto page)
        => _quotationRepository.ListBookAsync(Arg.Any<QuotationBookFilter>(), Arg.Any<CancellationToken>())
            .Returns(page);

    [Fact]
    [Trait("RuleId", "RN-077")]
    public async Task Execute_DeveRecusar_QuandoSemCorretoraAtiva()
    {
        var act = async () => await _useCase.ExecuteAsync(
            new ListQuotationBookRequest { ActiveBrokerageId = null }, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
        await _quotationRepository.DidNotReceiveWithAnyArgs().ListBookAsync(default!, default);
    }

    [Fact]
    public async Task Execute_DeveSanearPaginacao()
    {
        ArrangeBook(Page());

        var result = await _useCase.ExecuteAsync(
            new ListQuotationBookRequest { ActiveBrokerageId = _brokerageId, Page = 0, PageSize = 500 },
            CancellationToken.None);

        result.Page.Should().Be(1);
        result.PageSize.Should().Be(100);
        await _quotationRepository.Received(1).ListBookAsync(
            Arg.Is<QuotationBookFilter>(f => f.BrokerageId == _brokerageId && f.Page == 1 && f.PageSize == 100),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-078")]
    public async Task Execute_DeveFiltrarPorSituacao_PeloNomeEstavel()
    {
        ArrangeBook(Page());

        await _useCase.ExecuteAsync(
            new ListQuotationBookRequest { ActiveBrokerageId = _brokerageId, Situation = "analysis" },
            CancellationToken.None);

        await _quotationRepository.Received(1).ListBookAsync(
            Arg.Is<QuotationBookFilter>(f => f.Situation == EQuotationResult.Analysis),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("RuleId", "RN-078")]
    public async Task Execute_DeveRecusarSituacaoInvalida()
    {
        var act = async () => await _useCase.ExecuteAsync(
            new ListQuotationBookRequest { ActiveBrokerageId = _brokerageId, Situation = "Arquivada" },
            CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        await _quotationRepository.DidNotReceiveWithAnyArgs().ListBookAsync(default!, default);
    }

    [Fact]
    [Trait("RuleId", "RN-077")]
    public async Task Execute_DeveMapearLinha_ComSeguradoraESituacaoPorNomeEstavel()
    {
        var item = Item(requiresCcg: true);
        ArrangeBook(Page([item]));

        var result = await _useCase.ExecuteAsync(
            new ListQuotationBookRequest { ActiveBrokerageId = _brokerageId }, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        var mapped = result.Items[0];
        mapped.InsurerName.Should().Be("Newe Seguros");
        mapped.InsurerLogoUrl.Should().Be("https://cdn/newe.png");
        mapped.ModalityName.Should().Be("Executante Fornecedor");
        mapped.Result.Should().Be("ReadyForEmission");
        mapped.Premium.Should().Be(18_000m);
        // RN-058/059: a exigência de CCG é ortogonal ao resultado e trafega na linha (badge no front).
        mapped.RequiresCcg.Should().BeTrue();
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    [Trait("RuleId", "RN-078")]
    public async Task Execute_DeveMapearContagemPorSituacao_PeloNomeEstavel()
    {
        ArrangeBook(Page(counts:
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
    [Trait("RuleId", "RN-077")]
    public async Task Execute_DeveMapearOpcoesDeFiltro()
    {
        var insurerId = Guid.NewGuid();
        var modalityId = Guid.NewGuid();
        ArrangeBook(Page(
            insurers: [new QuotationBookOptionDto(insurerId, "Newe Seguros")],
            modalities: [new QuotationBookOptionDto(modalityId, "Executante Fornecedor")]));

        var result = await _useCase.ExecuteAsync(
            new ListQuotationBookRequest { ActiveBrokerageId = _brokerageId }, CancellationToken.None);

        result.Insurers.Should().ContainSingle(o => o.Id == insurerId && o.Name == "Newe Seguros");
        result.Modalities.Should().ContainSingle(o => o.Id == modalityId && o.Name == "Executante Fornecedor");
    }

    [Fact]
    [Trait("RuleId", "RN-077")]
    public async Task Execute_DeveRepassarFiltrosAvancados()
    {
        ArrangeBook(Page());
        var insurerId = Guid.NewGuid();
        var modalityId = Guid.NewGuid();

        await _useCase.ExecuteAsync(
            new ListQuotationBookRequest
            {
                ActiveBrokerageId = _brokerageId,
                Search = "pilão",
                InsurerId = insurerId,
                ModalityId = modalityId,
                PremiumMin = 1_000m,
                PremiumMax = 50_000m,
                InsuredAmountMin = 100_000m,
                CreatedFrom = new DateOnly(2026, 7, 1),
                CoverageStartFrom = new DateOnly(2026, 7, 15),
            },
            CancellationToken.None);

        await _quotationRepository.Received(1).ListBookAsync(
            Arg.Is<QuotationBookFilter>(f =>
                f.Search == "pilão"
                && f.InsurerId == insurerId
                && f.ModalityId == modalityId
                && f.PremiumMin == 1_000m
                && f.PremiumMax == 50_000m
                && f.InsuredAmountMin == 100_000m
                && f.CreatedFrom == new DateOnly(2026, 7, 1)
                && f.CoverageStartFrom == new DateOnly(2026, 7, 15)),
            Arg.Any<CancellationToken>());
    }
}
