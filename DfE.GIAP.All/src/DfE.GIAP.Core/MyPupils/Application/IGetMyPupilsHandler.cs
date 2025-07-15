using DfE.GIAP.Core.MyPupils.Domain;
using DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.AuthorisationContext;

namespace DfE.GIAP.Core.MyPupils.Application;
public interface IGetMyPupilsHandler
{
    Task<IEnumerable<MyPupil>> Get(MyPupilsAuthorisationContext context);
}
