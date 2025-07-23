using DfE.GIAP.Core.MyPupils.Domain.Aggregate;
using DfE.GIAP.Core.MyPupils.Domain.Authorisation;
using DfE.GIAP.Core.MyPupils.Domain.Entities;

namespace DfE.GIAP.Core.MyPupils.Domain.Services;
public interface IAggregatePupilsForMyPupilsDomainService
{
    Task<IEnumerable<Pupil>> GetPupilsAsync(
        IEnumerable<PupilIdentifier> pupilIdentifiers,
        PupilAuthorisationContext authorisationContext,
        PupilSelectionDomainCriteria pupilSelectionCriteria);
}
