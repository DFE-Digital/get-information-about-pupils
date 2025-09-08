using Xunit;
using Moq;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;
using DfE.GIAP.Web.Features.MyPupils.State;
using DfE.GIAP.Web.Session.Abstraction.Query;
using DfE.GIAP.Web.Tests.TestDoubles.Session;
using DfE.GIAP.Web.Tests.TestDoubles.MyPupils;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.State;

public sealed class GetMyPupilsStateHandlerTests
{
    [Fact]
    public void Constructor_Throws_When_PresentationStateHandler_Is_Null()
    {
        Mock<ISessionQueryHandler<MyPupilsPupilSelectionState>> selectionStateHandler =
            ISessionQueryHandlerTestDoubles.Default<MyPupilsPupilSelectionState>();

        Assert.Throws<ArgumentNullException>(() =>
            new GetMyPupilsStateHandler(null, selectionStateHandler.Object));
    }

    [Fact]
    public void Constructor_Throws_When_SelectionStateHandler_Is_Null()
    {
        Mock<ISessionQueryHandler<MyPupilsPresentationState>> presentationHandler =
            ISessionQueryHandlerTestDoubles.Default<MyPupilsPresentationState>();

        Assert.Throws<ArgumentNullException>(() =>
            new GetMyPupilsStateHandler(presentationHandler.Object, null!));
    }

    [Fact]
    public void GetState_ReturnsDefaultStates_WhenSessionResponsesAreEmpty()
    {
        SessionQueryResponse<MyPupilsPresentationState> presentationState =
            SessionQueryResponse<MyPupilsPresentationState>.NoValue();

        Mock<ISessionQueryHandler<MyPupilsPresentationState>> presentationHandlerMock =
            ISessionQueryHandlerTestDoubles.MockFor(presentationState);

        SessionQueryResponse<MyPupilsPupilSelectionState> selectionState =
            SessionQueryResponse<MyPupilsPupilSelectionState>.NoValue();

        Mock<ISessionQueryHandler<MyPupilsPupilSelectionState>> selectionStateHandlerMock =
            ISessionQueryHandlerTestDoubles.MockFor(selectionState);

        GetMyPupilsStateHandler handler = new(
            presentationHandlerMock.Object,
            selectionStateHandlerMock.Object);

        MyPupilsState result = handler.GetState();

        Assert.NotNull(result);
        Assert.Equivalent(MyPupilsPresentationState.CreateDefault(), result.PresentationState);
        Assert.Equivalent(MyPupilsPupilSelectionState.CreateDefault(), result.SelectionState);
        Assert.Empty(result.SelectionState.GetPupilsWithSelectionState());
    }

    [Fact]
    public void GetState_ReturnsStatesFromSession_WhenAvailable()
    {
        MyPupilsPresentationState expectedPresentationState =
            MyPupilsPresentationStateTestDoubles.Create(
                page: 1,
                sortKey: "SORT_KEY",
                sortDirection: SortDirection.Ascending);

        Mock<ISessionQueryHandler<MyPupilsPresentationState>> presentationHandler =
            ISessionQueryHandlerTestDoubles.MockFor(expectedPresentationState);

        List<UniquePupilNumber> upns = UniquePupilNumberTestDoubles.Generate(count: 10);

        MyPupilsPupilSelectionState expectedSelectionState = MyPupilsPupilSelectionStateTestDoubles.WithSelectionState(
            new()
            {
                { upns.Take(5).ToList(), true },
                { upns.Skip(5).ToList(), false }
            });

        Mock<ISessionQueryHandler<MyPupilsPupilSelectionState>> selectionStateHandler =
            ISessionQueryHandlerTestDoubles.MockFor<MyPupilsPupilSelectionState>(expectedSelectionState);

        GetMyPupilsStateHandler handler = new(
            presentationHandler.Object,
            selectionStateHandler.Object);

        MyPupilsState result = handler.GetState();

        Assert.NotNull(result);
        Assert.Equal(expectedPresentationState, result.PresentationState);
        Assert.Equal(expectedSelectionState, result.SelectionState);
    }
}
