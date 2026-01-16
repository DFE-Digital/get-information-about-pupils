using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.Models;
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
                AddCensusAutumnRecord(collection, pupil);
            if (selectedDatasets.Contains(Dataset.Census_Summer) && pupil.HasCensusSummerData)
                AddCensusSummerRecord(collection, pupil);
            if (selectedDatasets.Contains(Dataset.Census_Spring) && pupil.HasCensusSpringData)
                AddCensusSpringRecord(collection, pupil);
            if (selectedDatasets.Contains(Dataset.KS1) && pupil.HasKeyStage1Data)
                AddKS1Record(collection, pupil);
            if (selectedDatasets.Contains(Dataset.KS2) && pupil.HasKeyStage2Data)
                AddKS2Record(collection, pupil);
            if (selectedDatasets.Contains(Dataset.KS4) && pupil.HasKeyStage4Data)
                AddKS4Record(collection, pupil);
            if (selectedDatasets.Contains(Dataset.MTC) && pupil.HasMtcData)
                AddMTCRecord(collection, pupil);
            if (selectedDatasets.Contains(Dataset.Phonics) && pupil.HasPhonicsData)
                AddPhonicsRecord(collection, pupil);
            if (selectedDatasets.Contains(Dataset.EYFSP) && pupil.HasEYFSPData)
                AddEYFSPRecord(collection, pupil);

        }

        return collection;
    }

    private static void AddCensusAutumnRecord(PupilDatasetCollection collection, NationalPupil pp)
    {

    }

    private static void AddCensusSummerRecord(PupilDatasetCollection collection, NationalPupil pp)
    {

    }

    private static void AddCensusSpringRecord(PupilDatasetCollection collection, NationalPupil pp)
    {

    }

    private static void AddKS1Record(PupilDatasetCollection collection, NationalPupil pp)
    {

    }

    private static void AddKS2Record(PupilDatasetCollection collection, NationalPupil pp)
    {

    }

    private static void AddKS4Record(PupilDatasetCollection collection, NationalPupil pp)
    {

    }

    private static void AddMTCRecord(PupilDatasetCollection collection, NationalPupil pp)
    {

    }

    private static void AddPhonicsRecord(PupilDatasetCollection collection, NationalPupil pp)
    {

    }

    private static void AddEYFSPRecord(PupilDatasetCollection collection, NationalPupil pp)
    {

    }
}
