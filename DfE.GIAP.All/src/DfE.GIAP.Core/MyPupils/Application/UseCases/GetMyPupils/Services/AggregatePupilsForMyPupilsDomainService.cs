using DfE.GIAP.Core.MyPupils.Domain.Authorisation;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.Services;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services;
internal sealed class AggregatePupilsForMyPupilsDomainService : IAggregatePupilsForMyPupilsDomainService
{
    public AggregatePupilsForMyPupilsDomainService() // CognitiveSearchAbstraction(s) to query an index which hydrates a model we pass it?
    {
    }

    public Task<IEnumerable<Pupil>> GetPupilsAsync(
        IEnumerable<UniquePupilNumber> upns,
        PupilAuthorisationContext authorisationContext)
    {
        // Call Cognitive Search abstraction for Types,
        // Create Domain Pupil either via Mapper, using authorisationContext.ShouldMaskPupil()
        return Task.FromResult(Array.Empty<Pupil>().AsEnumerable());
    }
}

