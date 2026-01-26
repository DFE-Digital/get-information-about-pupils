using DfE.GIAP.Core.Downloads.Application.Aggregators;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Repositories;

namespace DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers;

public class NationalPupilDatabaseAggregationHandler : IPupilDatasetAggregationHandler
{
    public DownloadType SupportedDownloadType => DownloadType.NPD;
    private readonly INationalPupilReadOnlyRepository _npdReadRepository;
    private readonly IMapper<NationalPupil, CensusAutumnOutput> _autumnMapper;
    private readonly IMapper<NationalPupil, CensusSummerOutput> _summerMapper;
    private readonly IMapper<NationalPupil, CensusSpringOutput> _springMapper;
    private readonly IMapper<NationalPupil, KS1Output> _ks1Mapper;
    private readonly IMapper<NationalPupil, KS2Output> _ks2Mapper;
    private readonly IMapper<NationalPupil, KS4Output> _ks4Mapper;
    private readonly IMapper<NationalPupil, MTCOutput> _mtcMapper;
    private readonly IMapper<NationalPupil, PhonicsOutput> _phonicsMapper;
    private readonly IMapper<NationalPupil, EYFSPOutput> _eyfspMapper;
    public NationalPupilDatabaseAggregationHandler(
        INationalPupilReadOnlyRepository npdReadRepository,
        IMapper<NationalPupil, CensusAutumnOutput> autumnMapper,
        IMapper<NationalPupil, CensusSummerOutput> summerMapper,
        IMapper<NationalPupil, CensusSpringOutput> springMapper,
        IMapper<NationalPupil, KS1Output> ks1Mapper,
        IMapper<NationalPupil, KS2Output> ks2Mapper,
        IMapper<NationalPupil, KS4Output> ks4Mapper,
        IMapper<NationalPupil, MTCOutput> mtcMapper,
        IMapper<NationalPupil, PhonicsOutput> phonicsMapper,
        IMapper<NationalPupil, EYFSPOutput> eyfspMapper)
    {
        ArgumentNullException.ThrowIfNull(npdReadRepository);
        ArgumentNullException.ThrowIfNull(autumnMapper);
        ArgumentNullException.ThrowIfNull(summerMapper);
        ArgumentNullException.ThrowIfNull(springMapper);
        ArgumentNullException.ThrowIfNull(ks1Mapper);
        ArgumentNullException.ThrowIfNull(ks2Mapper);
        ArgumentNullException.ThrowIfNull(ks4Mapper);
        ArgumentNullException.ThrowIfNull(mtcMapper);
        ArgumentNullException.ThrowIfNull(phonicsMapper);
        ArgumentNullException.ThrowIfNull(eyfspMapper);
        _npdReadRepository = npdReadRepository;
        _autumnMapper = autumnMapper;
        _summerMapper = summerMapper;
        _springMapper = springMapper;
        _ks1Mapper = ks1Mapper;
        _ks2Mapper = ks2Mapper;
        _ks4Mapper = ks4Mapper;
        _mtcMapper = mtcMapper;
        _phonicsMapper = phonicsMapper;
        _eyfspMapper = eyfspMapper;
    }

    public async Task<PupilDatasetCollection> AggregateAsync(
        IEnumerable<string> pupilIds,
        IEnumerable<Dataset> selectedDatasets,
        CancellationToken cancellationToken = default)
    {
        PupilDatasetCollection collection = new();
        IEnumerable<NationalPupil> pupils = await _npdReadRepository.GetPupilsByIdsAsync(pupilIds);

        foreach (NationalPupil pupil in pupils)
        {
            if (selectedDatasets.Contains(Dataset.Census_Autumn) && pupil.HasCensusAutumnData)
                collection.CensusAutumn.Add(_autumnMapper.Map(pupil));
            if (selectedDatasets.Contains(Dataset.Census_Summer) && pupil.HasCensusSummerData)
                collection.CensusSummer.Add(_summerMapper.Map(pupil));
            if (selectedDatasets.Contains(Dataset.Census_Spring) && pupil.HasCensusSpringData)
                collection.CensusSpring.Add(_springMapper.Map(pupil));
            if (selectedDatasets.Contains(Dataset.KS1) && pupil.HasKeyStage1Data)
                collection.KS1.Add(_ks1Mapper.Map(pupil));
            if (selectedDatasets.Contains(Dataset.KS2) && pupil.HasKeyStage2Data)
                collection.KS2.Add(_ks2Mapper.Map(pupil));
            if (selectedDatasets.Contains(Dataset.KS4) && pupil.HasKeyStage4Data)
                collection.KS4.Add(_ks4Mapper.Map(pupil));
            if (selectedDatasets.Contains(Dataset.MTC) && pupil.HasMtcData)
                collection.MTC.Add(_mtcMapper.Map(pupil));
            if (selectedDatasets.Contains(Dataset.Phonics) && pupil.HasPhonicsData)
                collection.Phonics.Add(_phonicsMapper.Map(pupil));
            if (selectedDatasets.Contains(Dataset.EYFSP) && pupil.HasEYFSPData)
                collection.EYFSP.Add(_eyfspMapper.Map(pupil));
        }

        return collection;
    }
}
