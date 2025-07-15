using DfE.GIAP.Core.MyPupils.Domain;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.Pupil.Domain;

namespace DfE.GIAP.Core.MyPupils.Application.Repository;
internal interface IMyPupilReadOnlyRepository
{
    Task<IEnumerable<MyPupil>> GetMyPupilsById(
        IEnumerable<UniquePupilIdentifier> upns,
        MyPupilsAuthorisationContext context,
        CancellationToken cancellationToken = default);
}
