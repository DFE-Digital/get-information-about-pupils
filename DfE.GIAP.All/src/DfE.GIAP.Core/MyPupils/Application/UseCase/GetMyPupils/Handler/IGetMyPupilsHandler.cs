using DfE.GIAP.Core.MyPupils.Domain;
using DfE.GIAP.Core.MyPupils.Domain.Entities;

namespace DfE.GIAP.Core.MyPupils.Application.UseCase.GetMyPupils.Handler;
public interface IGetMyPupilsHandler
{
    Task<IEnumerable<MyPupil>> HandleAsync(
        MyPupilsAuthorisationContext context,
        IEnumerable<string> urns,
        CancellationToken ctx = default);
}
