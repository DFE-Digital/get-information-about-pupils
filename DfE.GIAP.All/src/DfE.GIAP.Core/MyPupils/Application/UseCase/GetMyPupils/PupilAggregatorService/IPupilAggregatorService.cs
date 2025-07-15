using DfE.GIAP.Core.Common.Domain.Pupil;

namespace DfE.GIAP.Core.MyPupils.Application.UseCase.GetMyPupils.PupilAggregatorService;
public interface IPupilAggregatorService
{
    public Task<IEnumerable<Pupil>> GetPupils(IEnumerable<string> upns, CancellationToken ctx = default); // May become a Dictionary<Npd|Fe|Pp, IEnumerable<Pupils>>, or a FineGrained service per each. Not sure yet.
}
