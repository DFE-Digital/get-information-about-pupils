using DfE.GIAP.Core.Downloads.Application.DataDownloads.Aggregators;
using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application.DataDownloads.Aggregators.Handlers;

public interface IPupilDatasetAggregationHandler
{
    PupilDownloadType SupportedDownloadType { get; }

    Task<PupilDatasetCollection> AggregateAsync(
        IEnumerable<string> pupilIds,
        IEnumerable<Dataset> selectedDatasets,
        CancellationToken cancellationToken = default);
}
