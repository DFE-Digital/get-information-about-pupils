using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Repositories;

namespace DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers;

public class NationalPupilDatabaseAggregationHandler : IPupilDatasetAggregationHandler
{
    public DownloadType SupportedDownloadType => DownloadType.NPD;

    private readonly INationalPupilReadOnlyRepository _npdReadRepository;
    private readonly IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>> _autumnMapper;
    private readonly IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>> _summerMapper;
    private readonly IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>> _springMapper;
    private readonly IMapper<NationalPupil, IEnumerable<KS1OutputRecord>> _ks1Mapper;
    private readonly IMapper<NationalPupil, IEnumerable<KS2OutputRecord>> _ks2Mapper;
    private readonly IMapper<NationalPupil, IEnumerable<KS4OutputRecord>> _ks4Mapper;
    private readonly IMapper<NationalPupil, IEnumerable<MTCOutputRecord>> _mtcMapper;
    private readonly IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>> _phonicsMapper;
    private readonly IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>> _eyfspMapper;

    public NationalPupilDatabaseAggregationHandler(
        INationalPupilReadOnlyRepository npdReadRepository,
        IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>> autumnMapper,
        IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>> summerMapper,
        IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>> springMapper,
        IMapper<NationalPupil, IEnumerable<KS1OutputRecord>> ks1Mapper,
        IMapper<NationalPupil, IEnumerable<KS2OutputRecord>> ks2Mapper,
        IMapper<NationalPupil, IEnumerable<KS4OutputRecord>> ks4Mapper,
        IMapper<NationalPupil, IEnumerable<MTCOutputRecord>> mtcMapper,
        IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>> phonicsMapper,
        IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>> eyfspMapper)
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
                collection.CensusAutumn.AddRange(_autumnMapper.Map(pupil));
            if (selectedDatasets.Contains(Dataset.Census_Summer) && pupil.HasCensusSummerData)
                collection.CensusSummer.AddRange(_summerMapper.Map(pupil));
            if (selectedDatasets.Contains(Dataset.Census_Spring) && pupil.HasCensusSpringData)
                collection.CensusSpring.AddRange(_springMapper.Map(pupil));
            if (selectedDatasets.Contains(Dataset.KS1) && pupil.HasKeyStage1Data)
                collection.KS1.AddRange(_ks1Mapper.Map(pupil));
            if (selectedDatasets.Contains(Dataset.KS2) && pupil.HasKeyStage2Data)
                collection.KS2.AddRange(_ks2Mapper.Map(pupil));
            if (selectedDatasets.Contains(Dataset.KS4) && pupil.HasKeyStage4Data)
                collection.KS4.AddRange(_ks4Mapper.Map(pupil));
            if (selectedDatasets.Contains(Dataset.MTC) && pupil.HasMtcData)
                collection.MTC.AddRange(_mtcMapper.Map(pupil));
            if (selectedDatasets.Contains(Dataset.Phonics) && pupil.HasPhonicsData)
                collection.Phonics.AddRange(_phonicsMapper.Map(pupil));
            if (selectedDatasets.Contains(Dataset.EYFSP) && pupil.HasEYFSPData)
                collection.EYFSP.AddRange(_eyfspMapper.Map(pupil));
        }

        return collection;
    }
}
