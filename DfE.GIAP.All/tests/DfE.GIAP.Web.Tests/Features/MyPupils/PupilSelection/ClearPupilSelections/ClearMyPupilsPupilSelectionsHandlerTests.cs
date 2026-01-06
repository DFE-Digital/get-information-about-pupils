using DfE.GIAP.Web.Features.MyPupils.SelectionState;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.ClearSelections;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.GetPupilSelections;
using DfE.GIAP.Web.Tests.TestDoubles.Session;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.PupilSelection.ClearPupilSelections;
public sealed class ClearMyPupilsPupilSelectionsHandlerTests
{
    [Fact]
    public void Constructor_Throws_When_StateProvider_Is_Null()
    {
        // Arrange
        Func<ClearMyPupilsPupilSelectionsHandler> construct =
            () => new(
                null!,
                ISessionCommandHandlerTestDoubles.Default<MyPupilsPupilSelectionState>().Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_SessionCommandHandler_Is_Null()
    {
        Func<ClearMyPupilsPupilSelectionsHandler> construct =
            () => new(
                new Mock<IGetMyPupilsPupilSelectionProvider>().Object,
                null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);

    }
}
