using DfE.GIAP.Web.Features.MyPupils.Services.UpdateMyPupilsState.PupilSelectionStateUpdater;
using Moq;

namespace DfE.GIAP.Web.Tests.TestDoubles.MyPupils;
internal static class IPupilSelectionStateUpdateHandlerTestDoubles
{
    internal static Mock<IPupilSelectionStateUpdateHandler> Default() => new();
}
