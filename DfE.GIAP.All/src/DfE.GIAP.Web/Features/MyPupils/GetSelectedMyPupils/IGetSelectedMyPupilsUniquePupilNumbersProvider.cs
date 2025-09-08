using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Web.Features.MyPupils.GetSelectedMyPupils;

public interface IGetSelectedMyPupilsUniquePupilNumbersProvider
{
    UniquePupilNumbers GetSelectedMyPupils();
}
