using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using DfE.GIAP.Core.Downloads.Application.DataDownloads.FileExports;
using DfE.GIAP.Core.Downloads.Application.Enums;

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

        // Build header row using Display / DisplayName attributes
        IEnumerable<string?> headers = props.Select(p =>
        {
            DisplayAttribute? display = p.GetCustomAttribute<DisplayAttribute>();
            if (display is not null)
                return display.Name;
            DisplayNameAttribute? displayName = p.GetCustomAttribute<DisplayNameAttribute>();
            if (displayName is not null)
                return displayName.DisplayName;

            return p.Name;
        });

        await writer.WriteLineAsync(string.Join(delimiter, headers));

        // Write rows
        foreach (T? record in records)
        {
            IEnumerable<string> values = props.Select(p => p.GetValue(record)?.ToString() ?? string.Empty);
            await writer.WriteLineAsync(string.Join(delimiter, values));
        }

        await writer.FlushAsync();
    }
}
