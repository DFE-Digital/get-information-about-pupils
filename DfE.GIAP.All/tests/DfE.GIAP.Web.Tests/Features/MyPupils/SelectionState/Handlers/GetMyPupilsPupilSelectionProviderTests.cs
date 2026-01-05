using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.Handlers;
using DfE.GIAP.Web.Session.Abstraction.Query;
using DfE.GIAP.Web.Tests.Features.MyPupils.TestDoubles;
using DfE.GIAP.Web.Tests.TestDoubles.Session;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.SelectionState.Handlers;

public sealed class GetMyPupilsPupilSelectionProviderTests
{
    [Fact]
    public void Constructor_Throws_When_PresentationStateHandler_Is_Null()
    {
        // Act Assert
        Assert.Throws<ArgumentNullException>(() =>
            new GetMyPupilsPupilSelectionProvider(null));
    }

    [Fact]
    public void GetState_ReturnsDefaultStates_WhenSessionResponsesAreEmpty()
    {
        // Arrange
        SessionQueryResponse<MyPupilsPupilSelectionState> selectionState =
            SessionQueryResponse<MyPupilsPupilSelectionState>.CreateWithNoValue();

        Mock<ISessionQueryHandler<MyPupilsPupilSelectionState>> selectionStateHandlerMock =
            ISessionQueryHandlerTestDoubles.MockFor(selectionState);

        GetMyPupilsPupilSelectionProvider sut = new(selectionStateHandlerMock.Object);

        // Act
        MyPupilsPupilSelectionState result = sut.GetPupilSelections();

        // Assert
        Assert.NotNull(result);
        Assert.Equivalent(MyPupilsPupilSelectionState.CreateDefault(), result);
    }

    [Fact]
    public void GetState_ReturnsStatesFromSession_WhenAvailable()
    {
        // Arrange
        List<string> upns =
            UniquePupilNumberTestDoubles.Generate(count: 10)
                .Select(t => t.Value)
                .ToList();

        List<string> selectedPupils = upns.Take(5).ToList();

        MyPupilsPupilSelectionState expectedSelectionState =
            MyPupilsPupilSelectionStateTestDoubles.WithPupilsSelectionState(
                SelectionMode.None,
                selected: selectedPupils,
                deselected: upns.Skip(5).ToList());

        Mock<ISessionQueryHandler<MyPupilsPupilSelectionState>> selectionStateHandler =
            ISessionQueryHandlerTestDoubles.MockFor(expectedSelectionState);

        GetMyPupilsPupilSelectionProvider sut = new(selectionStateHandler.Object);

        // Act
        MyPupilsPupilSelectionState response = sut.GetPupilSelections();

        // Assert
        Assert.NotNull(response);
        Assert.Equal(expectedSelectionState.Mode, response.Mode);
        Assert.Equal(expectedSelectionState.IsAnyPupilSelected, response.IsAnyPupilSelected);
        Assert.Equivalent(expectedSelectionState.GetExplicitSelections(), response.GetExplicitSelections());
        Assert.Equivalent(expectedSelectionState.GetDeselectedExceptions(), response.GetDeselectedExceptions());
    }

    // TODO store mode as SelectAll, retrieves
}
