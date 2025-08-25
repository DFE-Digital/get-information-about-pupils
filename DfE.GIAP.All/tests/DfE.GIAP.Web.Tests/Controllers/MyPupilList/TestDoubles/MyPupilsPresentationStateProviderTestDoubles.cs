using DfE.GIAP.Web.Controllers.MyPupilList.PresentationState.Provider;
using Moq;

namespace DfE.GIAP.Web.Tests.Controllers.MyPupilList.TestDoubles;
internal static class MyPupilsPresentationStateProviderTestDoubles
{
    internal static Mock<IMyPupilsPresentationStateProvider> Default() => new();
}
