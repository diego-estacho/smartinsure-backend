using ClosedXML.Excel;
using SmartInsure.Core.Abstractions.Services;

namespace SmartInsure.Infra.CrossCutting.Export;

/// <summary>
/// Exportação genérica para .xlsx (v1 síncrona) com ClosedXML: cabeçalhos em negrito
/// na primeira linha e uma linha por item. Não vaza tipos do ClosedXML para fora.
/// </summary>
public sealed class ClosedXmlExporter : IExcelExporter
{
    public byte[] Export<T>(IEnumerable<T> rows, IReadOnlyList<ExcelColumn<T>> columns, string sheetName)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet(sheetName);

        for (var column = 0; column < columns.Count; column++)
        {
            var headerCell = worksheet.Cell(1, column + 1);
            headerCell.Value = columns[column].Header;
            headerCell.Style.Font.Bold = true;
        }

        var rowIndex = 2;

        foreach (var item in rows)
        {
            for (var column = 0; column < columns.Count; column++)
            {
                var cell = worksheet.Cell(rowIndex, column + 1);
                WriteValue(cell, columns[column].Value(item));
            }

            rowIndex++;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return stream.ToArray();
    }

    private static void WriteValue(IXLCell cell, object? value)
    {
        switch (value)
        {
            case null:
                cell.Value = Blank.Value;
                break;
            case DateTime dateTime:
                cell.Value = dateTime;
                cell.Style.DateFormat.Format = "dd/MM/yyyy";
                break;
            case DateTimeOffset dateTimeOffset:
                cell.Value = dateTimeOffset.DateTime;
                cell.Style.DateFormat.Format = "dd/MM/yyyy";
                break;
            case bool boolean:
                cell.Value = boolean;
                break;
            case sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal:
                cell.Value = Convert.ToDouble(value);
                break;
            default:
                cell.Value = value.ToString();
                break;
        }
    }
}
