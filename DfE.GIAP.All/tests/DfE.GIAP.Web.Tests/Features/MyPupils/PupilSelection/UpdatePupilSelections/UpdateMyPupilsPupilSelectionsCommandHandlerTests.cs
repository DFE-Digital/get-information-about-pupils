using DfE.GIAP.Web.Features.MyPupils.Areas.UpdateForm;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.GetPupilSelections;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.UpdatePupilSelections;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.PupilSelection.UpdatePupilSelections;
public sealed class UpdateMyPupilsPupilSelectionsCommandHandlerTests
{
    [Fact]
    public void Constuctor_Throws_When_Provider_Is_Null()
    {
        // Arrange
        Func<UpdateMyPupilsPupilSelectionsCommandHandler> construct =
            () => new(
                null!,
                ISessionCommandHandlerTestDoubles.Default<MyPupilsPupilSelectionState>().Object,
                new Mock<IEvaluator<UpdateMyPupilsSelectionStateRequest>>().Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constuctor_Throws_When_SessionHandler_Is_Null()
    {
        // Arrange
        Func<UpdateMyPupilsPupilSelectionsCommandHandler> construct =
            () => new(
                new Mock<IGetMyPupilsPupilSelectionProvider>().Object,
                null!,
                new Mock<IEvaluator<UpdateMyPupilsSelectionStateRequest>>().Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constuctor_Throws_When_EvaluationHandler_Is_Null()
    {
        // Arrange
        Func<UpdateMyPupilsPupilSelectionsCommandHandler> construct =
            () => new(
                new Mock<IGetMyPupilsPupilSelectionProvider>().Object,
                ISessionCommandHandlerTestDoubles.Default<MyPupilsPupilSelectionState>().Object,
                null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task Handle_Throws_When_Request_Is_Null()
    {
        Mock<IGetMyPupilsPupilSelectionProvider> providerMock = new();

        Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> selectionStateCommandHandler =
            ISessionCommandHandlerTestDoubles.Default<MyPupilsPupilSelectionState>();

        Mock<IEvaluator<UpdateMyPupilsSelectionStateRequest>> evaluationHandlerMock = new();

        UpdateMyPupilsPupilSelectionsCommandHandler sut = new(
            providerMock.Object,
            selectionStateCommandHandler.Object,
            evaluationHandlerMock.Object);

        // Act Assert
        Func<Task> act = () => sut.Handle(null!);

        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    [Fact]
    public async Task Handle_Calls_Evaluator_And_Calls_SessionHandler()
    {
        MyPupilsPupilSelectionState defaultState = MyPupilsPupilSelectionState.CreateDefault();

        Mock<IGetMyPupilsPupilSelectionProvider> providerMock = new();
        providerMock
            .Setup(t => t.GetPupilSelections())
            .Returns(defaultState);

        Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> selectionStateCommandHandler =
            ISessionCommandHandlerTestDoubles.Default<MyPupilsPupilSelectionState>();

        Mock<IEvaluator<UpdateMyPupilsSelectionStateRequest>> evaluatorMock = new();

        // Arrange
        UpdateMyPupilsPupilSelectionsCommandHandler sut = new(
                providerMock.Object,
                selectionStateCommandHandler.Object,
                evaluatorMock.Object);

        MyPupilsFormStateRequestDto request = new();

        // Act
        await sut.Handle(request);

        // Assert
        providerMock.Verify(t => t.GetPupilSelections(), Times.Once);

        evaluatorMock.Verify((handler)
            => handler.EvaluateAsync(
                It.Is<UpdateMyPupilsSelectionStateRequest>(
                    (req) => ReferenceEquals(request, req.UpdateRequest) && ReferenceEquals(defaultState, req.State)),
                It.IsAny<EvaluationOptions>(),
                It.IsAny<CancellationToken>()),
                Times.Once);

        selectionStateCommandHandler.Verify(handler =>
            handler.StoreInSession(
                It.Is<MyPupilsPupilSelectionState>((store) => ReferenceEquals(defaultState, store))),
            Times.Once);
    }
}
