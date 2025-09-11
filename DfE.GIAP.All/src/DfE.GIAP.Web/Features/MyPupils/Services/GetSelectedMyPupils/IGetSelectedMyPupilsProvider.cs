using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Web.Features.MyPupils.Services.GetSelectedMyPupils;

public interface IGetSelectedMyPupilsProvider
{
    UniquePupilNumbers GetSelectedMyPupils();
}
