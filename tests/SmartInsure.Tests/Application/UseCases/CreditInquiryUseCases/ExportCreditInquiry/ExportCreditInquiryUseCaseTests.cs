using ClosedXML.Excel;
using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ExportCreditInquiry;
using SmartInsure.Application.UseCase.UseCases.CreditInquiryUseCases.ExportCreditInquiry.Requests;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Exceptions;
using SmartInsure.Infra.CrossCutting.Export;

namespace SmartInsure.Tests.Application.UseCases.CreditInquiryUseCases.ExportCreditInquiry;

/// <summary>
/// RN-201 — Exportação do quadro consolidado de uma Consulta de Crédito (.xlsx).
/// Usa o <see cref="ClosedXmlExporter"/> real e relê o arquivo para validar a projeção ponta a ponta.
/// </summary>
[Trait("RuleId", "RN-201")]
public class ExportCreditInquiryUseCaseTests
{
    private const string XlsxContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private const string ValidCnpj = "12345678000195";

    private static readonly Guid BrokerageId = Guid.CreateVersion7();
    private static readonly Guid InsurerHighId = Guid.CreateVersion7();
    private static readonly Guid InsurerLowId = Guid.CreateVersion7();
    private static readonly Guid InsurerDownId = Guid.CreateVersion7();

    private readonly ICreditInquiryRepository _creditInquiryRepository =
        Substitute.For<ICreditInquiryRepository>();

    private readonly IInsurerRepository _insurerRepository =
        Substitute.For<IInsurerRepository>();

    private readonly ExportCreditInquiryUseCase _useCase;

    public ExportCreditInquiryUseCaseTests()
    {
        _useCase = new ExportCreditInquiryUseCase(
            _creditInquiryRepository, _insurerRepository, new ClosedXmlExporter());
    }

    private CreditInquiry BuildInquiryWithResults()
    {
        var inquiryId = Guid.CreateVersion7();
        var inquiry = CreditInquiry.Create(BrokerageId, ValidCnpj);
        typeof(EntityBase).GetProperty("Id")!.SetValue(inquiry, inquiryId);

        // Disponível com maior limite (líder Financeira=3000, revisado 3500 → utilizado 500).
        inquiry.AddResult(CreditInquiryResult.Available(
            inquiryId, InsurerHighId,
            new[]
            {
                CreditInquiryResultLimit.Create("Tradicional", "GARANTIA_TRADICIONAL", 1000m, 1200m, 0.20m),
                CreditInquiryResultLimit.Create("Judicial", "GARANTIA_JUDICIAL", 2000m, 2000m, 0.30m),
                CreditInquiryResultLimit.Create("JudicialFiscal", "GARANTIA_JUDICIAL_FISCAL", 2000m, 2000m, 0.35m),
                CreditInquiryResultLimit.Create("Financeira", "GARANTIA_FINANCEIRA", 3000m, 3500m, 0.40m),
            },
            responseTimeMs: 1500));

        // Disponível com limite menor (só Tradicional=500).
        inquiry.AddResult(CreditInquiryResult.Available(
            inquiryId, InsurerLowId,
            new[] { CreditInquiryResultLimit.Create("Tradicional", "GARANTIA_TRADICIONAL", 500m, 500m, 0.10m) },
            responseTimeMs: 800));

        // Indisponível — sem limites, com motivo.
        inquiry.AddResult(CreditInquiryResult.Unavailable(inquiryId, InsurerDownId, "Sistema indisponível"));

        _creditInquiryRepository.GetByIdAsync(inquiry.Id, Arg.Any<CancellationToken>()).Returns(inquiry);

        _insurerRepository
            .GetCorporateNamesByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, string>
            {
                [InsurerHighId] = "Seguradora Alta",
                [InsurerLowId] = "Seguradora Baixa",
                [InsurerDownId] = "Seguradora Fora",
            });

        return inquiry;
    }

    [Fact]
    public async Task Execute_DeveLancarNotFound_QuandoConsultaNaoEncontrada()
    {
        _creditInquiryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((CreditInquiry?)null);

        var act = () => _useCase.ExecuteAsync(
            new ExportCreditInquiryRequest(Guid.CreateVersion7()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Consulta de crédito não encontrada*");
    }

    [Fact]
    public async Task Execute_DeveNomearArquivoComCnpj_EDevolverXlsx()
    {
        var inquiry = BuildInquiryWithResults();

        var response = await _useCase.ExecuteAsync(
            new ExportCreditInquiryRequest(inquiry.Id), CancellationToken.None);

        response.FileName.Should().Be($"consulta-credito-{ValidCnpj}.xlsx");
        response.ContentType.Should().Be(XlsxContentType);
        response.Content.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Execute_DeveGerarCabecalhoDeOnzeColunas_EUmaLinhaPorSeguradora()
    {
        var inquiry = BuildInquiryWithResults();

        var response = await _useCase.ExecuteAsync(
            new ExportCreditInquiryRequest(inquiry.Id), CancellationToken.None);

        using var workbook = new XLWorkbook(new MemoryStream(response.Content));
        var sheet = workbook.Worksheets.First();

        sheet.Name.Should().Be("Consulta de crédito");
        sheet.Cell(1, 1).GetString().Should().Be("Seguradora");
        sheet.Cell(1, 2).GetString().Should().Be("Status");
        sheet.Row(1).LastCellUsed().Address.ColumnNumber.Should().Be(11);

        // 1 cabeçalho + 3 seguradoras.
        sheet.RowsUsed().Count().Should().Be(4);
    }

    [Fact]
    public async Task Execute_DeveOrdenarAprovadosPorMaiorLimite_EComporAsColunas()
    {
        var inquiry = BuildInquiryWithResults();

        var response = await _useCase.ExecuteAsync(
            new ExportCreditInquiryRequest(inquiry.Id), CancellationToken.None);

        using var workbook = new XLWorkbook(new MemoryStream(response.Content));
        var sheet = workbook.Worksheets.First();

        // Ordenação da tela (RN-029): Alta (máx 3000) antes de Baixa (500); Indisponível por último.
        sheet.Cell(2, 1).GetString().Should().Be("Seguradora Alta");
        sheet.Cell(3, 1).GetString().Should().Be("Seguradora Baixa");
        sheet.Cell(4, 1).GetString().Should().Be("Seguradora Fora");

        // Status em 2 estados (RN-029).
        sheet.Cell(2, 2).GetString().Should().Be("Aprovado");
        sheet.Cell(4, 2).GetString().Should().Be("Indisponível");

        // Colunas fixas da linha Alta: Tradicional=1000, Judicial=2000 (grupo judicial), Financeira=3000.
        sheet.Cell(2, 3).GetValue<decimal>().Should().Be(1000m);
        sheet.Cell(2, 5).GetValue<decimal>().Should().Be(2000m);
        sheet.Cell(2, 7).GetValue<decimal>().Should().Be(3000m);

        // Utilizado do líder (Financeira): revisado 3500 − disponível 3000 = 500.
        sheet.Cell(2, 9).GetValue<decimal>().Should().Be(500m);

        // Indisponível: motivo presente, limites ausentes.
        sheet.Cell(4, 11).GetString().Should().Be("Sistema indisponível");
        sheet.Cell(4, 3).IsEmpty().Should().BeTrue();
    }
}
