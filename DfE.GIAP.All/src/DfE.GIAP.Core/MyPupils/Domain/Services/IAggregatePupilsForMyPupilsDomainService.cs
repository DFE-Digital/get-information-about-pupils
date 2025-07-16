using DfE.GIAP.Core.MyPupils.Domain.Authorisation;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Domain.Services;
public interface IAggregatePupilsForMyPupilsDomainService
{
    // May become a Dictionary<Npd|Fe|Pp, IEnumerable<Pupils>>, or a FineGrained service per each. Not sure yet.
    public Task<IEnumerable<Pupil>> GetPupilsAsync(
        IEnumerable<UniquePupilNumber> upns,
        MyPupilsAuthorisationContext authorisationContext);  
}
