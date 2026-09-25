using DfE.GIAP.Core.Downloads.Application.Models;

namespace DfE.GIAP.Core.Downloads.Application.Repositories;

public interface IPupilPremiumReadOnlyRepository
{
    Task<IEnumerable<PupilPremiumPupil>> GetPupilsByIdsAsync(IEnumerable<string> pupilIds);
}
