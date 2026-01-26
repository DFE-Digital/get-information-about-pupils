using DfE.GIAP.Core.Downloads.Application.Models;

namespace DfE.GIAP.Core.Downloads.Application.Repositories;

public interface INationalPupilReadOnlyRepository
{
    Task<IEnumerable<NationalPupil>> GetPupilsByIdsAsync(IEnumerable<string> pupilIds);
}
