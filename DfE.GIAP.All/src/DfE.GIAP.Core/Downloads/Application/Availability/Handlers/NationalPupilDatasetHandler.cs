using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Repositories;

namespace DfE.GIAP.Core.Downloads.Application.Availability.Handlers;

public class NationalPupilDatasetHandler : IDatasetAvailabilityHandler
{
    public PupilDownloadType SupportedDownloadType => PupilDownloadType.NPD;
    private readonly INationalPupilReadOnlyRepository _nationalPupilDatabaseReadOnlyRepository;

    public NationalPupilDatasetHandler(INationalPupilReadOnlyRepository nationalPupilDatabaseReadOnlyRepository)
    {
        ArgumentNullException.ThrowIfNull(nationalPupilDatabaseReadOnlyRepository);
        _nationalPupilDatabaseReadOnlyRepository = nationalPupilDatabaseReadOnlyRepository;
    }

    public async Task<IEnumerable<Dataset>> GetAvailableDatasetsAsync(IEnumerable<string> pupilIds)
    {
        HashSet<Dataset> datasets = new();
        IEnumerable<NationalPupil> pupils = await _nationalPupilDatabaseReadOnlyRepository.GetPupilsByIdsAsync(pupilIds);

        if (pupils.Any(p => p.HasCensusAutumnData))
            datasets.Add(Dataset.Census_Autumn);
        if (pupils.Any(p => p.HasCensusSpringData))
            datasets.Add(Dataset.Census_Spring);
        if (pupils.Any(p => p.HasCensusSummerData))
            datasets.Add(Dataset.Census_Summer);
        if (pupils.Any(p => p.HasEYFSPData))
            datasets.Add(Dataset.EYFSP);
        if (pupils.Any(p => p.HasPhonicsData))
            datasets.Add(Dataset.Phonics);
        if (pupils.Any(p => p.HasKeyStage1Data))
            datasets.Add(Dataset.KS1);
        if (pupils.Any(p => p.HasKeyStage2Data))
            datasets.Add(Dataset.KS2);
        if (pupils.Any(p => p.HasKeyStage4Data))
            datasets.Add(Dataset.KS4);
        if (pupils.Any(p => p.HasMtcData))
            datasets.Add(Dataset.MTC);

        return datasets;
    }
}
