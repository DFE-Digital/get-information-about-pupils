using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;

namespace DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilDatasets;

public class DownloadPupilDataUseCase : IUseCase<DownloadPupilDataRequest, DownloadPupilDataResponse>
{
    private readonly IPupilDatasetAggregator _pupilDatasetAggregator;
    private readonly IDelimitedFileExporter _fileExporter;
    private readonly IZipArchiveBuilder _zipArchiveBuilder;

    public DownloadPupilDataUseCase(
        IPupilDatasetAggregator downloadDatasetAggregator,
        IDelimitedFileExporter fileExporter,
        IZipArchiveBuilder zipArchiveBuilder)
    {
        _pupilDatasetAggregator = downloadDatasetAggregator;
        _fileExporter = fileExporter;
        _zipArchiveBuilder = zipArchiveBuilder;
    }

    public async Task<DownloadPupilDataResponse> HandleRequestAsync(DownloadPupilDataRequest request)
    {
        PupilDatasetCollection datasets = await _pupilDatasetAggregator
            .AggregateAsync(request.DownloadType, request.SelectedPupils, request.SelectedDatasets);

        Dictionary<string, Func<Stream, Task>> fileWriters = new();

        void AddFile<T>(IEnumerable<T> records, string fileName)
        {
            if (records.Any())
            {
                fileWriters.Add(fileName, stream => _fileExporter.ExportAsync(records, request.FileFormat, stream));
            }
        }

        string ext = request.FileFormat == FileFormat.Csv ? "csv" : "txt";
        AddFile(datasets.PP, $"pp_results.{ext}");
        AddFile(datasets.SEN, $"sen_results.{ext}");

        // No data at all → return null
        if (!fileWriters.Any())
            return default!;

        // One dataset file → return it directly
        if (fileWriters.Count == 1)
        {
            (string? fileName, Func<Stream, Task>? writer) = fileWriters.First();

            using MemoryStream ms = new MemoryStream();
            await writer(ms);

            return new DownloadPupilDataResponse(ms.ToArray(), fileName!, "application/octet-stream");
        }

        // Multiple dataset files → zip them
        byte[] zipBytes = await _zipArchiveBuilder.CreateZipAsync(fileWriters);
        string zipFileName = request.DownloadType switch
        {
            DownloadType.NPD => "npd_results.zip",
            DownloadType.PupilPremium => "pp_results.zip",
            DownloadType.FurtherEducation => "fe_results.zip",
            _ => $"{request.DownloadType.ToString().ToLower()}_results.zip"
        };

        return new DownloadPupilDataResponse(zipBytes, zipFileName, "application/zip");
    }
}
