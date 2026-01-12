using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Downloads.Application.Datasets;
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

        Dictionary<string, Func<Stream, Task>> fileWriters = BuildFileWriters(datasets, request);
        if (!fileWriters.Any())
            return default!;

        if (fileWriters.Count == 1)
            return await BuildSingleFileResponse(fileWriters, request);

        return await BuildZipResponse(fileWriters, request);
    }


    // -------------------------
    // Helpers
    // -------------------------
    private Dictionary<string, Func<Stream, Task>> BuildFileWriters(
        PupilDatasetCollection datasets,
        DownloadPupilDataRequest request)
    {
        Dictionary<string, Func<Stream, Task>> writers = [];

        DatasetMetadata metadata = DatasetMetadata.For(request.DownloadType);
        foreach (Dataset dataset in request.SelectedDatasets)
        {
            if (!metadata.SupportedDatasets.Contains(dataset))
                continue;

            List<object> records = metadata.GetRecords(dataset, datasets).ToList();
            if (!records.Any())
                continue;

            string fileName = metadata.GetFileName(dataset, request.FileFormat);
            writers.Add(fileName, stream =>
                _fileExporter.ExportAsync(records, request.FileFormat, stream));
        }

        return writers;
    }

    private static async Task<DownloadPupilDataResponse> BuildSingleFileResponse(
        Dictionary<string, Func<Stream, Task>> fileWriters,
        DownloadPupilDataRequest request)
    {
        (string? fileName, Func<Stream, Task>? writer) = fileWriters.First();

        using MemoryStream ms = new();
        await writer(ms);

        return new DownloadPupilDataResponse(
            ms.ToArray(),
            fileName,
            GetContentType(request.FileFormat));
    }

    private async Task<DownloadPupilDataResponse> BuildZipResponse(
        Dictionary<string, Func<Stream, Task>> fileWriters,
        DownloadPupilDataRequest request)
    {
        byte[] zipBytes = await _zipArchiveBuilder.CreateZipAsync(fileWriters);

        string zipName = request.DownloadType switch
        {
            DownloadType.NPD => "npd_results.zip",
            DownloadType.PupilPremium => "pp_results.zip",
            DownloadType.FurtherEducation => "fe_results.zip",
            _ => $"{request.DownloadType.ToString().ToLower()}_results.zip"
        };

        return new DownloadPupilDataResponse(zipBytes, zipName, "application/zip");
    }

    private static string GetContentType(FileFormat format) =>
        format == FileFormat.Csv ? "text/csv" : "text/plain";
}

