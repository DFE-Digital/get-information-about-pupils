using DfE.GIAP.SharedTests.Runtime.TestDoubles;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.ClearPupilSelections;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.Options;
using DfE.GIAP.Web.Shared.Session.Abstraction;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.PupilSelection.ClearPupilSelections;
public sealed class ClearMyPupilsPupilSelectionsHandlerTests
{
    [Fact]
    public void Constructor_Throws_When_SessionCommandHandler_Is_Null()
    {
        Func<ClearMyPupilsPupilSelectionsHandler> construct =
            () => new(null!, OptionsTestDoubles.Default<MyPupilSelectionOptions>());

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_Options_Is_Null()
    {
        Func<ClearMyPupilsPupilSelectionsHandler> construct =
            () => new(new Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>>().Object, null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_OptionsValue_Is_Null()
    {
        Func<ClearMyPupilsPupilSelectionsHandler> construct = () => new(
            new Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>>().Object,
            OptionsTestDoubles.MockNullOptions<MyPupilSelectionOptions>());

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Handle_Calls_SessionCommandHandler_With_Default_State()
    {
        // Arrange
        Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> handlerMock = ISessionCommandHandlerTestDoubles.Default<MyPupilsPupilSelectionState>();

        IOptions<MyPupilSelectionOptions> options = OptionsTestDoubles.Default<MyPupilSelectionOptions>();

        ClearMyPupilsPupilSelectionsHandler sut = new(handlerMock.Object, options);

        // Act
        sut.Handle();

        // Assert
        MyPupilsPupilSelectionState defaultState = MyPupilsPupilSelectionState.CreateDefault();

        handlerMock.Verify((handler) =>
            handler.StoreInSession(
                It.Is<SessionCacheKey>(cacheKey => cacheKey.Value == options.Value.SelectionsSessionKey),
                It.Is<MyPupilsPupilSelectionState>(
                    (t) => t.Mode == defaultState.Mode &&
                        t.GetManualSelections().SequenceEqual(defaultState.GetManualSelections()) &&
                            t.GetDeselectedExceptions().SequenceEqual(defaultState.GetDeselectedExceptions()))),
            Times.Once);
    }
}
