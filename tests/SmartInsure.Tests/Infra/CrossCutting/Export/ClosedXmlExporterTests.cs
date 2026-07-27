using ClosedXML.Excel;
using FluentAssertions;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Infra.CrossCutting.Export;

namespace SmartInsure.Tests.Infra.CrossCutting.Export;

/// <summary>Exportação genérica para .xlsx com ClosedXML: cabeçalhos + uma linha por item.</summary>
public class ClosedXmlExporterTests
{
    private readonly ClosedXmlExporter _exporter = new();

    private sealed record Row(string Name, int Count);

    [Fact]
    public void Export_DeveGerarPlanilhaComCabecalhoEDados_QuandoHaLinhas()
    {
        IReadOnlyList<ExcelColumn<Row>> columns =
        [
            new("Nome", row => row.Name),
            new("Quantidade", row => row.Count),
        ];

        var rows = new[]
        {
            new Row("Alfa", 1),
            new Row("Beta", 2),
        };

        var bytes = _exporter.Export(rows, columns, "Dados");

        bytes.Should().NotBeNullOrEmpty();

        using var workbook = new XLWorkbook(new MemoryStream(bytes));
        var worksheet = workbook.Worksheet(1);

        worksheet.Cell(1, 1).GetString().Should().Be("Nome");
        worksheet.Cell(1, 2).GetString().Should().Be("Quantidade");
        worksheet.LastRowUsed()!.RowNumber().Should().Be(3);
        worksheet.Cell(2, 1).GetString().Should().Be("Alfa");
        worksheet.Cell(3, 1).GetString().Should().Be("Beta");
    }
}
