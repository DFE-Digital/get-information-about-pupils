using System.Reflection;
using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application;

public interface IDelimitedFileExporter
{
    Task ExportAsync<T>(IEnumerable<T> records, FileFormat format, Stream output);
}

public class DelimitedFileExporter : IDelimitedFileExporter
{
    public async Task ExportAsync<T>(
        IEnumerable<T> records,
        FileFormat format,
        Stream output)
    {
        string delimiter = format is FileFormat.Csv ? "," : "\t";
        PropertyInfo[] props = typeof(T).GetProperties();

        using StreamWriter writer = new(output, leaveOpen: true);

        // Header
        await writer.WriteLineAsync(string.Join(delimiter, props.Select(p => p.Name)));

        // Rows
        foreach (T? record in records)
        {
            IEnumerable<string> values = props.Select(p => p.GetValue(record)?.ToString() ?? string.Empty);
            await writer.WriteLineAsync(string.Join(delimiter, values));
        }

        await writer.FlushAsync();
    }
}
