using DfE.GIAP.Web.Features.MyPupils.SelectionState;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.ClearSelections;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.GetPupilSelections;
using DfE.GIAP.Web.Shared.Session.Abstraction.Command;
using DfE.GIAP.Web.Tests.Shared.Session.TestDoubles;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.PupilSelection.ClearPupilSelections;
public sealed class ClearMyPupilsPupilSelectionsHandlerTests
{
    [Fact]
    public void Constructor_Throws_When_SessionCommandHandler_Is_Null()
    {
        Func<ClearMyPupilsPupilSelectionsHandler> construct = () => new(null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Handle_Calls_SessionCommandHandler_With_Default_State()
    {
        // Arrange
        Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> handlerMock = ISessionCommandHandlerTestDoubles.Default<MyPupilsPupilSelectionState>();

        ClearMyPupilsPupilSelectionsHandler sut = new(handlerMock.Object);

        // Act
        sut.Handle();

        // Assert
        MyPupilsPupilSelectionState defaultState = MyPupilsPupilSelectionState.CreateDefault();

        handlerMock.Verify((handler) =>
            handler.StoreInSession(It.Is<MyPupilsPupilSelectionState>(
                (t) => t.Mode == defaultState.Mode &&
                    t.GetManualSelections().SequenceEqual(defaultState.GetManualSelections()) &&
                        t.GetDeselectedExceptions().SequenceEqual(defaultState.GetDeselectedExceptions()))),
            Times.Once);
    }
}
