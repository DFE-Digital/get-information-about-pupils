using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Repositories;

namespace DfE.GIAP.Core.Downloads.Application.Availability.Handlers;

public class FurtherEducationDatasetHandler : IDatasetAvailabilityHandler
{
    public DownloadType SupportedDownloadType => DownloadType.FurtherEducation;
    private readonly IFurtherEducationReadOnlyRepository _furtherEducationReadOnlyRepository;

    public FurtherEducationDatasetHandler(IFurtherEducationReadOnlyRepository furtherEducationReadOnlyRepository)
    {
        ArgumentNullException.ThrowIfNull(furtherEducationReadOnlyRepository);
        _furtherEducationReadOnlyRepository = furtherEducationReadOnlyRepository;
    }

    public async Task<IEnumerable<Dataset>> GetAvailableDatasetsAsync(IEnumerable<string> pupilIds)
    {
        HashSet<Dataset> datasets = new();
        IEnumerable<FurtherEducationPupil> pupils = await _furtherEducationReadOnlyRepository.GetPupilsByIdsAsync(pupilIds);

        if (pupils.Any(p => p.HasPupilPremiumData))
            datasets.Add(Dataset.FE_PP);
        if (pupils.Any(p => p.HasSpecialEducationalNeedsData))
            datasets.Add(Dataset.SEN);

        return datasets;
    }
}
