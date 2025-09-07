using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser.UpdateCurrentPageOfPupilsHandler;

public interface IUpdateCurrentPageOfPupilsStateHandler
{
    void Handle(UniquePupilNumbers pupils);
}
