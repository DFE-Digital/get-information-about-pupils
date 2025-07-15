using DfE.GIAP.Core.Common.Domain.Pupil;

namespace DfE.GIAP.Core.MyPupils.Application.UseCase.GetMyPupils.PupilAggregatorService;
internal sealed class PupilAggregatorService : IPupilAggregatorService
{
    public Task<IEnumerable<Pupil>> GetPupils(IEnumerable<string> upns, CancellationToken ctx = default)
    {
        throw new NotImplementedException();
    }
}
