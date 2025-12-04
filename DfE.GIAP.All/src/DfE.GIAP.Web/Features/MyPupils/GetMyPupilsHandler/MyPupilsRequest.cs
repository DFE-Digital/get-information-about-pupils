using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.Web.Features.MyPupils.State;

namespace DfE.GIAP.Web.Features.MyPupils.GetPupilViewModels;

public record MyPupilsRequest
{
    public MyPupilsRequest(string userId, MyPupilsState state)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        UserId = userId;

        ArgumentNullException.ThrowIfNull(state);
        State = state;
    }

    public string UserId { get; }

    public MyPupilsState State { get; }
}
