using DfE.GIAP.Core.Pupil.Domain;

namespace DfE.GIAP.Core.MyPupils.Application.UseCase.GetMyPupils.Services;
internal sealed class PupilAggregatorService : IPupilAggregatorService
{
    public Task<IEnumerable<Pupil.Domain.Pupil>> GetPupils(
        IEnumerable<UniquePupilIdentifier> upns,
        CancellationToken ctx = default)
    {
        return Task.FromResult(Enumerable.Empty<Pupil.Domain.Pupil>());
    }
}
