using DfE.GIAP.Core.Common.Domain.Pupil;

namespace DfE.GIAP.Core.MyPupils.Domain.Services.PupilAggregator;
public interface IPupilAggregatorService
{
    public IEnumerable<Pupil> GetPupils(IEnumerable<string> upns); // May become a Dictionary<Npd|Fe|Pp, IEnumerable<Pupils>>, or a FineGrained service per each. Not sure yet.
}
