using DfE.GIAP.Core.Downloads.Application.DataDownloads.Aggregators;
using DfE.GIAP.Core.Downloads.Application.DataDownloads.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Repositories;

namespace DfE.GIAP.Core.Downloads.Application.DataDownloads.Aggregators.Handlers;

public class PupilPremiumAggregationHandler : IPupilDatasetAggregationHandler
{
    public DownloadType SupportedDownloadType => DownloadType.PupilPremium;

    private readonly IPupilPremiumReadOnlyRepository _ppReadRepository;
    private readonly IMapper<PupilPremiumPupil, IEnumerable<PupilPremiumOutputRecord>> _ppMapper;

    public PupilPremiumAggregationHandler(
        IPupilPremiumReadOnlyRepository pupilPremiumReadRepository,
        IMapper<PupilPremiumPupil, IEnumerable<PupilPremiumOutputRecord>> ppMapper)
    {
        ArgumentNullException.ThrowIfNull(pupilPremiumReadRepository);
        ArgumentNullException.ThrowIfNull(ppMapper);
        _ppReadRepository = pupilPremiumReadRepository;
        _ppMapper = ppMapper;
    }

    public async Task<PupilDatasetCollection> AggregateAsync(
            IEnumerable<string> pupilIds,
            IEnumerable<Dataset> selectedDatasets,
            CancellationToken cancellationToken = default)
    {
        IEnumerable<PupilPremiumPupil> pupils = await _ppReadRepository.GetPupilsByIdsAsync(pupilIds);
        PupilDatasetCollection collection = new();

        foreach (PupilPremiumPupil pupil in pupils)
        {
            if (selectedDatasets.Contains(Dataset.PP) && pupil.HasPupilPremiumData)
                collection.PupilPremium.AddRange(_ppMapper.Map(pupil));
        }

        return collection;
    }
}
