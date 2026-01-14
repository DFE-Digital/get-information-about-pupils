using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.Repositories;

namespace DfE.GIAP.Core.Downloads.Application.Pupils.Aggregators.Handlers;

public class NationalPupilDatabaseAggregationHandler : IPupilDatasetAggregationHandler
{
    public bool CanHandle(DownloadType downloadType)
        => downloadType == DownloadType.NPD;

    private readonly INationalPupilReadOnlyRepository _npdReadRepository;
    public NationalPupilDatabaseAggregationHandler(INationalPupilReadOnlyRepository npdReadRepository)
    {
        ArgumentNullException.ThrowIfNull(npdReadRepository);
        _npdReadRepository = npdReadRepository;
    }

    public Task<PupilDatasetCollection> AggregateAsync(
        IEnumerable<string> pupilIds,
        IEnumerable<Dataset> selectedDatasets,
        CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
