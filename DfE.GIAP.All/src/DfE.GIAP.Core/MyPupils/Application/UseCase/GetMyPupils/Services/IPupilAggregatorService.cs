using DfE.GIAP.Core.Pupil.Domain;

namespace DfE.GIAP.Core.MyPupils.Application.UseCase.GetMyPupils.Services;
public interface IPupilAggregatorService
{
    public Task<IEnumerable<Pupil.Domain.Pupil>> GetPupils(
        IEnumerable<UniquePupilIdentifier> upns,
        CancellationToken ctx = default); // May become a Dictionary<Npd|Fe|Pp, IEnumerable<Pupils>>, or a FineGrained service per each. Not sure yet.
}
