using DfE.GIAP.Core.Downloads.Application.DataDownloads.Aggregators;
using DfE.GIAP.Core.Downloads.Application.DataDownloads.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Repositories;

namespace DfE.GIAP.Core.Downloads.Application.DataDownloads.Aggregators.Handlers;

public class FurtherEducationAggregationHandler : IPupilDatasetAggregationHandler
{
    public DownloadType SupportedDownloadType => DownloadType.FurtherEducation;

    private readonly IFurtherEducationReadOnlyRepository _feReadRepository;
    private readonly IMapper<FurtherEducationPupil, IEnumerable<FurtherEducationPPOutputRecord>> _ppMapper;
    private readonly IMapper<FurtherEducationPupil, IEnumerable<FurtherEducationSENOutputRecord>> _senMapper;

    public FurtherEducationAggregationHandler(
        IFurtherEducationReadOnlyRepository feReadRepository,
        IMapper<FurtherEducationPupil, IEnumerable<FurtherEducationPPOutputRecord>> ppMapper,
        IMapper<FurtherEducationPupil, IEnumerable<FurtherEducationSENOutputRecord>> senMapper)
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
                collection.FurtherEducationPP.AddRange(_ppMapper.Map(pupil));

            if (selectedDatasets.Contains(Dataset.SEN) && pupil.HasSpecialEducationalNeedsData)
                collection.SEN.AddRange(_senMapper.Map(pupil));
        }

        return collection;
    }
}
