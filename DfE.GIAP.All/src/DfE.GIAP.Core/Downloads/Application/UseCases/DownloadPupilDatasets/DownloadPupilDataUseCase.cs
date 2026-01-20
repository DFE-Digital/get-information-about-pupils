using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Downloads.Application.Aggregators;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.FileExports;

namespace DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilDatasets;

public class DownloadPupilDataUseCase : IUseCase<DownloadPupilDataRequest, DownloadPupilDataResponse>
{
    private readonly IPupilDatasetAggregatorFactory _pupilDatasetAggregatorFactory;
    private readonly IDelimitedFileExporter _fileExporter;
    private readonly IZipArchiveBuilder _zipArchiveBuilder;

    public DownloadPupilDataUseCase(
        IPupilDatasetAggregatorFactory downloadDatasetAggregator,
        IDelimitedFileExporter fileExporter,
        IZipArchiveBuilder zipArchiveBuilder)
    {
        _pupilDatasetAggregatorFactory = downloadDatasetAggregator;
        _fileExporter = fileExporter;
        _zipArchiveBuilder = zipArchiveBuilder;
    }

    public async Task<DownloadPupilDataResponse> HandleRequestAsync(DownloadPupilDataRequest request)
    {
        PupilDatasetCollection datasets = await _pupilDatasetAggregatorFactory
            .AggregateAsync(request.DownloadType, request.SelectedPupils, request.SelectedDatasets);

        Dictionary<string, Func<Stream, Task>> fileStreams = BuildFiles(datasets, request);
        if (!fileStreams.Any())
            return new DownloadPupilDataResponse();

        if (fileStreams.Count == 1)
            return await BuildSingleFileResponse(fileStreams, request);

        return await BuildZipResponse(fileStreams, request);
    }


    // -------------------------
    // Helpers
    // -------------------------
    private Dictionary<string, Func<Stream, Task>> BuildFiles(
        PupilDatasetCollection datasets,
        DownloadPupilDataRequest request)
    {
        Dictionary<string, Func<Stream, Task>> writers = new();
        foreach (Dataset dataset in request.SelectedDatasets)
        {
            List<object> records = datasets.GetRecords(dataset).ToList();
            if (!records.Any())
                continue;

            string baseName = GetBaseFileName(dataset);
            string ext = request.FileFormat == FileFormat.Csv ? "csv" : "txt";
            string fileName = $"{baseName}.{ext}";

            writers.Add(fileName, stream => _fileExporter.ExportAsync(records, request.FileFormat, stream));
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

    private static string GetBaseFileName(Dataset dataset) =>
        dataset switch
        {
            Dataset.FE_PP => "pp_results",
            Dataset.SEN => "sen_results",
            Dataset.PP => "pp_search_results",
            Dataset.KS1 => "ks1_results",
            Dataset.KS2 => "ks2_results",
            Dataset.KS4 => "ks4_results",
            Dataset.Census_Autumn => "census_autumn_results",
            Dataset.Census_Spring => "census_spring_results",
            Dataset.Census_Summer => "census_summer_results",
            Dataset.EYFSP => "eyfsp_results",
            Dataset.Phonics => "phonics_results",
            Dataset.MTC => "mtc_results",
            _ => "results"
        };

}

