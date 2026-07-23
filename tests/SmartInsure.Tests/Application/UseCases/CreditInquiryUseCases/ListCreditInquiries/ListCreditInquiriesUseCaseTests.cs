using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ListCreditInquiries;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ListCreditInquiries.Requests;
using SmartInsure.Core.Abstractions.Repositories;

namespace SmartInsure.Tests.Application.UseCases.CreditInquiryUseCases.ListCreditInquiries;

/// <summary>RN-031 — Listagem paginada de histórico de Consultas de Crédito com filtros opcionais.</summary>
[Trait("RuleId", "RN-031")]
public class ListCreditInquiriesUseCaseTests
{
    private static readonly Guid BrokerageId = Guid.CreateVersion7();
    private static readonly string ValidCnpj = "12345678000195";

    private readonly ICreditInquiryRepository _creditInquiryRepository =
        Substitute.For<ICreditInquiryRepository>();

    private readonly ListCreditInquiriesUseCase _useCase;

    public ListCreditInquiriesUseCaseTests()
    {
        _useCase = new ListCreditInquiriesUseCase(_creditInquiryRepository);
    }

    [Fact]
    public async Task Execute_DeveRetornarListaPaginada_QuandoRequisicaoValida()
    {
        var page = 1;
        var pageSize = 20;
        var items = new List<CreditInquiryListItem>
        {
            new(Guid.CreateVersion7(), BrokerageId, ValidCnpj, DateTime.UtcNow, 3, 2),
            new(Guid.CreateVersion7(), BrokerageId, "98765432000109", DateTime.UtcNow.AddHours(-1), 2, 1),
        };
        var total = 100L;

        _creditInquiryRepository.ListPagedAsync(
            Arg.Any<Guid?>(), Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns((items.AsReadOnly(), total));

        var request = new ListCreditInquiriesRequest
        {
            BrokerageId = null,
            PolicyHolderCnpj = null,
            Page = page,
            PageSize = pageSize,
        };

        var response = await _useCase.ExecuteAsync(request, CancellationToken.None);

        response.Items.Should().HaveCount(2);
        response.Page.Should().Be(page);
        response.PageSize.Should().Be(pageSize);
        response.Total.Should().Be(total);
    }

    [Fact]
    public async Task Execute_DeveRepassarFiltrosBrokerageIdAoRepositorio_QuandoFornecidos()
    {
        var request = new ListCreditInquiriesRequest
        {
            BrokerageId = BrokerageId,
            PolicyHolderCnpj = null,
            Page = 1,
            PageSize = 20,
        };

        _creditInquiryRepository.ListPagedAsync(
            BrokerageId, null, 1, 20, Arg.Any<CancellationToken>())
            .Returns((new List<CreditInquiryListItem>().AsReadOnly(), 0L));

        await _useCase.ExecuteAsync(request, CancellationToken.None);

        await _creditInquiryRepository.Received(1).ListPagedAsync(
            BrokerageId, null, 1, 20, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRepassarFiltroPolicyHolderCnpjAoRepositorio_QuandoFornecido()
    {
        var request = new ListCreditInquiriesRequest
        {
            BrokerageId = null,
            PolicyHolderCnpj = ValidCnpj,
            Page = 1,
            PageSize = 20,
        };

        _creditInquiryRepository.ListPagedAsync(
            null, ValidCnpj, 1, 20, Arg.Any<CancellationToken>())
            .Returns((new List<CreditInquiryListItem>().AsReadOnly(), 0L));

        await _useCase.ExecuteAsync(request, CancellationToken.None);

        await _creditInquiryRepository.Received(1).ListPagedAsync(
            null, ValidCnpj, 1, 20, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveRepassarPaginacaoAoRepositorio_QuandoPaginaEPageSizeEspecificados()
    {
        var request = new ListCreditInquiriesRequest
        {
            BrokerageId = null,
            PolicyHolderCnpj = null,
            Page = 5,
            PageSize = 50,
        };

        _creditInquiryRepository.ListPagedAsync(
            null, null, 5, 50, Arg.Any<CancellationToken>())
            .Returns((new List<CreditInquiryListItem>().AsReadOnly(), 0L));

        await _useCase.ExecuteAsync(request, CancellationToken.None);

        await _creditInquiryRepository.Received(1).ListPagedAsync(
            null, null, 5, 50, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeveMapearRespostas_QuandoItensRetornados()
    {
        var id1 = Guid.CreateVersion7();
        var id2 = Guid.CreateVersion7();
        var items = new List<CreditInquiryListItem>
        {
            new(id1, BrokerageId, ValidCnpj, DateTime.UtcNow, 3, 2),
            new(id2, BrokerageId, "98765432000109", DateTime.UtcNow.AddHours(-2), 1, 0),
        };

        _creditInquiryRepository.ListPagedAsync(
            Arg.Any<Guid?>(), Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns((items.AsReadOnly(), 2L));

        var request = new ListCreditInquiriesRequest { Page = 1, PageSize = 20 };
        var response = await _useCase.ExecuteAsync(request, CancellationToken.None);

        response.Items.Should().HaveCount(2);
        response.Items[0].Id.Should().Be(id1);
        response.Items[0].BrokerageId.Should().Be(BrokerageId);
        response.Items[0].PolicyHolderCnpj.Should().Be(ValidCnpj);
        response.Items[0].ResultsCount.Should().Be(3);
        response.Items[0].AvailableResults.Should().Be(2);

        response.Items[1].Id.Should().Be(id2);
        response.Items[1].ResultsCount.Should().Be(1);
        response.Items[1].AvailableResults.Should().Be(0);
    }

    [Fact]
    public async Task Execute_DeveRetornarListaVazia_QuandoNenhumaConsultaEncontrada()
    {
        _creditInquiryRepository.ListPagedAsync(
            Arg.Any<Guid?>(), Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns((new List<CreditInquiryListItem>().AsReadOnly(), 0L));

        var request = new ListCreditInquiriesRequest { Page = 1, PageSize = 20 };
        var response = await _useCase.ExecuteAsync(request, CancellationToken.None);

        response.Items.Should().BeEmpty();
        response.Total.Should().Be(0);
    }
}
