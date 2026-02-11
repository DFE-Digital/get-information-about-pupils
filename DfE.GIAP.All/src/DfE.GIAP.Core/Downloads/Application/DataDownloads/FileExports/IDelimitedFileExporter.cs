using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application.DataDownloads.FileExports;

public interface IDelimitedFileExporter
{
    Task ExportAsync<T>(IEnumerable<T> records, FileFormat format, Stream output);
}
