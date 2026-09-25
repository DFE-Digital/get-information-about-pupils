using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilDatasets.DataDownloads.Aggregators;

public interface IPupilDatasetAggregatorFactory
{
    Task<PupilDatasetCollection> AggregateAsync(
        PupilDownloadType downloadType,
        IEnumerable<string> pupilIds,
        IEnumerable<Dataset> selectedDatasets);
}
