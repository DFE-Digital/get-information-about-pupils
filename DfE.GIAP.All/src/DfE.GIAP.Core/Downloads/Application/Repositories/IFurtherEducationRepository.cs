using DfE.GIAP.Core.Downloads.Application.Models;

namespace DfE.GIAP.Core.Downloads.Application.Repositories;

public interface IFurtherEducationRepository
{
    Task<IEnumerable<FurtherEducationPupil>> GetPupilsByIdsAsync(IEnumerable<string> pupilIds);
}
