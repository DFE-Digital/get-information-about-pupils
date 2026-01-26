using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Repositories;

namespace DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers;

public class FurtherEducationAggregationHandler : IPupilDatasetAggregationHandler
{
    public DownloadType SupportedDownloadType => DownloadType.FurtherEducation;

    private readonly IFurtherEducationReadOnlyRepository _feReadRepository;
    private readonly IMapper<FurtherEducationPupil, FurtherEducationPPOutputRecord> _ppMapper;
    private readonly IMapper<FurtherEducationPupil, FurtherEducationSENOutputRecord> _senMapper;

    public FurtherEducationAggregationHandler(
        IFurtherEducationReadOnlyRepository feReadRepository,
        IMapper<FurtherEducationPupil, FurtherEducationPPOutputRecord> ppMapper,
        IMapper<FurtherEducationPupil, FurtherEducationSENOutputRecord> senMapper)
    {
        ArgumentNullException.ThrowIfNull(feReadRepository);
        ArgumentNullException.ThrowIfNull(ppMapper);
        ArgumentNullException.ThrowIfNull(senMapper);
        _feReadRepository = feReadRepository;
        _ppMapper = ppMapper;
        _senMapper = senMapper;
    }


    public async Task<PupilDatasetCollection> AggregateAsync(
            IEnumerable<string> pupilIds,
            IEnumerable<Dataset> selectedDatasets,
            CancellationToken cancellationToken = default)
    {
        PupilDatasetCollection collection = new();
        IEnumerable<FurtherEducationPupil> pupils = await _feReadRepository.GetPupilsByIdsAsync(pupilIds);

        foreach (FurtherEducationPupil pupil in pupils)
        {
            if (selectedDatasets.Contains(Dataset.FE_PP) && pupil.HasPupilPremiumData)
                collection.FurtherEducationPP.Add(_ppMapper.Map(pupil));
            if (selectedDatasets.Contains(Dataset.SEN) && pupil.HasSpecialEducationalNeedsData)
                collection.SEN.Add(_senMapper.Map(pupil));
        }

        return collection;
    }
}
