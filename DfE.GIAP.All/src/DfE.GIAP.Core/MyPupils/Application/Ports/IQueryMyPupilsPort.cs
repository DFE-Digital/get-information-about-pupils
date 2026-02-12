using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.MyPupils.Domain.Entities;

namespace DfE.GIAP.Core.MyPupils.Application.Ports;
public interface IQueryMyPupilsPort
{
    Task<IEnumerable<Pupil>> QueryAsync(UniquePupilNumbers myPupils, CancellationToken ctx = default);
}
