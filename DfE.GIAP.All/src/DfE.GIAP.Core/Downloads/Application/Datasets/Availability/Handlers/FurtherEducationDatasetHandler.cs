using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Repositories;

namespace DfE.GIAP.Core.Downloads.Application.Datasets.Availability.Handlers;

public class FurtherEducationDatasetHandler : IDatasetAvailabilityHandler
{
    public DownloadType SupportedDownloadType => DownloadType.FurtherEducation;
    private readonly IFurtherEducationRepository _repository;

    public FurtherEducationDatasetHandler(IFurtherEducationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Dataset>> GetAvailableDatasetsAsync(IEnumerable<string> pupilIds)
    {
        IReadOnlyCollection<Dataset> relevantDatasets = AvailableDatasetsByDownloadType
            .GetSupportedDatasets(SupportedDownloadType);

        HashSet<Dataset> datasets = new();
        IEnumerable<FurtherEducationPupil> pupils = await _repository.GetPupilsByIdsAsync(pupilIds);

        if (pupils.Any(p => p.HasPupilPremiumData))
            datasets.Add(Dataset.PP);
        if (pupils.Any(p => p.HasSpecialEducationalNeedsData))
            datasets.Add(Dataset.SEN);

        return datasets;
    }
}
