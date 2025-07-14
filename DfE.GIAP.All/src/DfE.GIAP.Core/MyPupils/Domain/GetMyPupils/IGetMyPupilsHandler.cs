using DfE.GIAP.Core.MyPupils.Domain.GetMyPupils.MaskPupilIdentifier.Rules.Abstraction;
using DfE.GIAP.Core.MyPupils.Domain.GetMyPupils.MaskPupilIdentifier.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Domain.GetMyPupils;
public interface IGetMyPupilsHander
{
    MyPupilsResponse Get(string[] upns, MyPupilsAuthorisationContext context);
}
