using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilDatasets.DataDownloads.Aggregators;

namespace DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilDatasets.DataDownloads.Aggregators.Handlers;

public interface IPupilDatasetAggregationHandler
{
    PupilDownloadType SupportedDownloadType { get; }

    Task<PupilDatasetCollection> AggregateAsync(
        IEnumerable<string> pupilIds,
        IEnumerable<Dataset> selectedDatasets,
        CancellationToken cancellationToken = default);
}
