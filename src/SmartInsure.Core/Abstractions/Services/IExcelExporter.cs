namespace SmartInsure.Core.Abstractions.Services;

public interface IExcelExporter
{
    byte[] Export<T>(IEnumerable<T> rows, IReadOnlyList<ExcelColumn<T>> columns, string sheetName);
}

public sealed record ExcelColumn<T>(string Header, Func<T, object?> Value);
