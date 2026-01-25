using DfE.GIAP.Core.Downloads.Application.Aggregators;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Repositories;

namespace DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers;

public class PupilPremiumAggregationHandler : IPupilDatasetAggregationHandler
{
    public DownloadType SupportedDownloadType => DownloadType.PupilPremium;

    private readonly IPupilPremiumDownloadDatasetReadOnlyRepository _ppReadRepository;
    private readonly IMapper<PupilPremiumPupil, PupilPremiumOutputRecord> _ppMapper;

    public PupilPremiumAggregationHandler(
        IPupilPremiumDownloadDatasetReadOnlyRepository pupilPremiumReadRepository,
        IMapper<PupilPremiumPupil, PupilPremiumOutputRecord> ppMapper)
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
                collection.PupilPremium.Add(_ppMapper.Map(pupil));
        }

        return collection;
    }
}
