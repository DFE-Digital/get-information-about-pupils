using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;
using DfE.GIAP.Web.Features.MyPupils.State;
using DfE.GIAP.Web.Features.MyPupils.State.Models;
using DfE.GIAP.Web.Features.MyPupils.State.Models.Selection;
using DfE.GIAP.Web.Session.Abstraction.Query;
using DfE.GIAP.Web.Tests.TestDoubles.MyPupils;
using DfE.GIAP.Web.Tests.TestDoubles.Session;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.State;

public sealed class GetMyPupilsStateProviderTests
{
    [Fact]
    public void Constructor_Throws_When_PresentationStateHandler_Is_Null()
    {
        Mock<ISessionQueryHandler<MyPupilsPupilSelectionState>> selectionStateHandler =
            ISessionQueryHandlerTestDoubles.Default<MyPupilsPupilSelectionState>();

        Assert.Throws<ArgumentNullException>(() =>
            new GetMyPupilsSelectionStateProvider(null, selectionStateHandler.Object));
    }

    [Fact]
    public void Constructor_Throws_When_SelectionStateHandler_Is_Null()
    {
        Mock<ISessionQueryHandler<MyPupilsPresentationQueryModel>> presentationHandler =
            ISessionQueryHandlerTestDoubles.Default<MyPupilsPresentationQueryModel>();

        Assert.Throws<ArgumentNullException>(() =>
            new GetMyPupilsSelectionStateProvider(presentationHandler.Object, null!));
    }

    [Fact]
    public void GetState_ReturnsDefaultStates_WhenSessionResponsesAreEmpty()
    {
        SessionQueryResponse<MyPupilsPresentationQueryModel> presentationState =
            SessionQueryResponse<MyPupilsPresentationQueryModel>.CreateWithNoValue();

        Mock<ISessionQueryHandler<MyPupilsPresentationQueryModel>> presentationHandlerMock =
            ISessionQueryHandlerTestDoubles.MockFor(presentationState);

        SessionQueryResponse<MyPupilsPupilSelectionState> selectionState =
            SessionQueryResponse<MyPupilsPupilSelectionState>.CreateWithNoValue();

        Mock<ISessionQueryHandler<MyPupilsPupilSelectionState>> selectionStateHandlerMock =
            ISessionQueryHandlerTestDoubles.MockFor(selectionState);

        GetMyPupilsPupilSelectionProvider handler = new(
            presentationHandlerMock.Object,
            selectionStateHandlerMock.Object);

        MyPupilsState result = handler.GetPupilSelections();

        Assert.NotNull(result);
        Assert.Equivalent(MyPupilsPresentationQueryModel.CreateDefault(), result.PresentationState);
        Assert.Equivalent(MyPupilsPupilSelectionState.CreateDefault(), result.SelectionState);
        Assert.Empty(result.SelectionState.GetPupilsWithSelectionState());
    }

    [Fact]
    public void GetState_ReturnsStatesFromSession_WhenAvailable()
    {
        MyPupilsPresentationQueryModel expectedPresentationState =
            MyPupilsPresentationStateTestDoubles.Create(
                page: 1,
                sortKey: "SORT_KEY",
                sortDirection: SortDirection.Ascending);

        Mock<ISessionQueryHandler<MyPupilsPresentationQueryModel>> presentationHandler =
            ISessionQueryHandlerTestDoubles.MockFor(expectedPresentationState);

        List<string> upns =
            UniquePupilNumberTestDoubles.Generate(count: 10)
                .Select(t => t.Value)
                .ToList();

        MyPupilsPupilSelectionState expectedSelectionState = MyPupilsPupilSelectionStateTestDoubles.WithPupilsSelectionState(
            new()
            {
                { upns.Take(5).ToList(), true },
                { upns.Skip(5).ToList(), false }
            });

        Mock<ISessionQueryHandler<MyPupilsPupilSelectionState>> selectionStateHandler =
            ISessionQueryHandlerTestDoubles.MockFor<MyPupilsPupilSelectionState>(expectedSelectionState);

        GetMyPupilsPupilSelectionProvider handler = new(
            presentationHandler.Object,
            selectionStateHandler.Object);

        MyPupilsState result = handler.GetPupilSelections();

        Assert.NotNull(result);
        Assert.Equal(expectedPresentationState, result.PresentationState);
        Assert.Equal(expectedSelectionState, result.SelectionState);
    }
}
