using DfE.GIAP.Core.Downloads.Application.Models;

namespace DfE.GIAP.Core.Downloads.Application.Repositories;

public interface IPupilPremiumDownloadDatasetReadOnlyRepository
{
    Task<IEnumerable<PupilPremiumPupil>> GetPupilsByIdsAsync(IEnumerable<string> pupilIds);
}
