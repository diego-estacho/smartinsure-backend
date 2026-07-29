using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SubmitQuotationTerms;
using SmartInsure.Application.UseCase.UseCases.QuotationUseCases.SubmitQuotationTerms.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Tests.Application.UseCases.QuotationUseCases.SubmitQuotationTerms;

/// <summary>
/// RN-063 — "Baixar minuta": envia os termos preenchidos (UpdateProposalTerms) e devolve a minuta
/// (GetProposalContractDraft). Exige proposta no provedor; id de cláusula não-numérico é dado inválido.
/// </summary>
[Trait("RuleId", "RN-063")]
public class SubmitQuotationTermsUseCaseTests
{
    private readonly IQuotationRepository _quotationRepository = Substitute.For<IQuotationRepository>();
    private readonly IPersonRepository _personRepository = Substitute.For<IPersonRepository>();
    private readonly IBrokerageInsurerEnablementRepository _enablementRepository =
        Substitute.For<IBrokerageInsurerEnablementRepository>();
    private readonly ICalculationEngine _engine = Substitute.For<ICalculationEngine>();

    private readonly Guid _brokerageId = Guid.CreateVersion7();
    private readonly Guid _insurerId = Guid.CreateVersion7();
    private readonly Guid _groupId = Guid.CreateVersion7();

    private SubmitQuotationTermsUseCase BuildUseCase()
    {
        var services = new ServiceCollection();
        services.AddKeyedSingleton<ICalculationEngine>(ECalculationEngine.PlugV2, (_, _) => _engine);

        return new SubmitQuotationTermsUseCase(
            _quotationRepository, _personRepository, _enablementRepository, services.BuildServiceProvider());
    }

    private Quotation ObtainedQuotation(string? proposalExternalId)
    {
        var quotation = Quotation.Requested(_groupId, _insurerId);
        quotation.MarkObtained(
            EQuotationResult.Automatic, null, 300m, null, null, null, null,
            proposalExternalId, "PN-1", false, null, false, [],
            new DateTime(2026, 7, 28, 12, 0, 0, DateTimeKind.Utc));
        return quotation;
    }

    private void SetupGraph(Quotation quotation)
    {
        _quotationRepository.GetByIdAsync(quotation.Id, Arg.Any<CancellationToken>()).Returns(quotation);
        _enablementRepository.GetByPairAsync(_brokerageId, _insurerId, Arg.Any<CancellationToken>())
            .Returns(BrokerageInsurerEnablement.Create(
                _brokerageId, _insurerId, ECalculationEngine.PlugV2, "{\"baseUrl\":\"https://x/\",\"key\":\"k\"}"));
        _personRepository.GetByIdAsync(_brokerageId, Arg.Any<CancellationToken>())
            .Returns(Person.Create("12345678000195", "Corretora", null, Guid.CreateVersion7()));
    }

    private SubmitQuotationTermsRequest Request(Quotation quotation, string clauseId = "10")
        => new(
            quotation.Id,
            _brokerageId,
            [new QuotationTermInput("objeto", "Fornecimento de bens")],
            [new QuotationClauseInput(clauseId, [new QuotationTermInput("percentual", "5")])]);

    [Fact]
    public async Task Execute_DeveEnviarTermosERetornarMinuta_QuandoCotacaoTemProposta()
    {
        var quotation = ObtainedQuotation("3fa85f64-5717-4562-b3fc-2c963f66afa6");
        SetupGraph(quotation);
        _engine.GetProposalContractDraftAsync(
                Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ProposalContractDraftResult(
                "https://x/draft.pdf", "draft-1", new DateTime(2026, 7, 28, 13, 0, 0, DateTimeKind.Utc)));

        var response = await BuildUseCase().ExecuteAsync(Request(quotation), CancellationToken.None);

        response.DraftUrl.Should().Be("https://x/draft.pdf");
        await _engine.Received(1).SubmitProposalTermsAsync(
            Arg.Any<string?>(), Arg.Any<SubmitProposalTermsInput>(), Arg.Any<CancellationToken>());
        await _engine.Received(1).GetProposalContractDraftAsync(
            Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoCotacaoSemPropostaNoProvedor()
    {
        var quotation = ObtainedQuotation(proposalExternalId: null);
        SetupGraph(quotation);

        var act = async () => await BuildUseCase().ExecuteAsync(Request(quotation), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        await _engine.DidNotReceive().SubmitProposalTermsAsync(
            Arg.Any<string?>(), Arg.Any<SubmitProposalTermsInput>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRecusar_QuandoIdDaClausulaNaoENumerico()
    {
        var quotation = ObtainedQuotation("3fa85f64-5717-4562-b3fc-2c963f66afa6");
        SetupGraph(quotation);

        var act = async () => await BuildUseCase().ExecuteAsync(
            Request(quotation, clauseId: "nao-numerico"), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }
}
