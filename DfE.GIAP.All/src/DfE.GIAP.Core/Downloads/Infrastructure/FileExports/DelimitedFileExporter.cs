using System.Reflection;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.FileExports;

namespace DfE.GIAP.Core.Downloads.Infrastructure.FileExports;

public class DelimitedFileExporter : IDelimitedFileExporter
{

    public async Task ExportAsync<T>(IEnumerable<T> records, FileFormat format, Stream output)
    {
        string delimiter = format == FileFormat.Csv ? "," : "\t";

        // Infer type from first record if T is object
        Type type = typeof(T);
        if (type == typeof(object) && records.Any())
        {
            type = records.First()!.GetType();
        }

        PropertyInfo[] props = type.GetProperties();

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
