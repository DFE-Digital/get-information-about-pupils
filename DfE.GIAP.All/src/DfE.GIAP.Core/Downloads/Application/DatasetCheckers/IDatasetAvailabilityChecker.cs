using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Repositories;

namespace DfE.GIAP.Core.Downloads.Application.DatasetCheckers;

public interface IDatasetAvailabilityChecker
{
    DownloadType SupportedDownloadType { get; }
    Task<IEnumerable<Datasets>> GetAvailableDatasetsAsync(IEnumerable<string> pupilIds);
}

public class FurtherEducationDatasetChecker : IDatasetAvailabilityChecker
{
    public DownloadType SupportedDownloadType => DownloadType.FurtherEducation;
    private readonly IFurtherEducationRepository _repository;

    public FurtherEducationDatasetChecker(IFurtherEducationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Datasets>> GetAvailableDatasetsAsync(IEnumerable<string> pupilIds)
    {
        // Define which datasets are relevant for FurtherEducation
        IReadOnlyCollection<Datasets> relevantDatasets = DownloadDatasetMap
            .GetSupportedDatasets(SupportedDownloadType);

        HashSet<Datasets> datasets = new();
        IEnumerable<FurtherEducationPupil> pupils = await _repository.GetPupilsByIdsAsync(pupilIds);

        foreach (FurtherEducationPupil pupil in pupils)
        {
            if (pupil.HasPupilPremiumData)
                datasets.Add(Datasets.PP);

            if (pupil.HasSpecialEducationalNeedsData)
                datasets.Add(Datasets.SEN);

            // Early exit: if all relevant datasets are found, break
            if (relevantDatasets.All(datasets.Contains))
                break;
        }

        return datasets;
    }
}
