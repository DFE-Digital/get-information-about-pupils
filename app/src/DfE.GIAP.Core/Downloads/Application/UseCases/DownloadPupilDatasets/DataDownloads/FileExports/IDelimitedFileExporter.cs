using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilDatasets.DataDownloads.FileExports;

public interface IDelimitedFileExporter
{
    Task ExportAsync<T>(IEnumerable<T> records, FileFormat format, Stream output);
}
