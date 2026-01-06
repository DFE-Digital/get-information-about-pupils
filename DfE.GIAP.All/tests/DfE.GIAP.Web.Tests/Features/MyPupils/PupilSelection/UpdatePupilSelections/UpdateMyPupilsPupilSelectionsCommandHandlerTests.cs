using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.CommandHandlers;
using DfE.GIAP.Web.Features.MyPupils.Areas.UpdateForm;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.UpdatePupilSelections;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.GetPupilSelections;
using DfE.GIAP.Web.Session.Abstraction.Command;
using DfE.GIAP.Web.Tests.TestDoubles.Session;
using Moq;
using Xunit;

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
                new Mock<IEvaluationHandler<UpdateMyPupilsSelectionStateRequest>>().Object);

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
                new Mock<IEvaluationHandler<UpdateMyPupilsSelectionStateRequest>>().Object);

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
    public void Handle_Throws_When_Request_Is_Null()
    {
        Mock<IGetMyPupilsPupilSelectionProvider> providerMock = new();

        Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> selectionStateCommandHandler =
            ISessionCommandHandlerTestDoubles.Default<MyPupilsPupilSelectionState>();

        Mock<IEvaluationHandler<UpdateMyPupilsSelectionStateRequest>> evaluationHandlerMock = new();

        UpdateMyPupilsPupilSelectionsCommandHandler sut = new(
            providerMock.Object,
            selectionStateCommandHandler.Object,
            evaluationHandlerMock.Object);

        // Act Assert
        Action act = () => sut.Handle(null!);

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Handle_Calls_Evaluator_And_Calls_SessionHandler()
    {
        MyPupilsPupilSelectionState defaultState = MyPupilsPupilSelectionState.CreateDefault();

        Mock<IGetMyPupilsPupilSelectionProvider> providerMock = new();
        providerMock
            .Setup(t => t.GetPupilSelections())
            .Returns(defaultState);

        Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> selectionStateCommandHandler =
            ISessionCommandHandlerTestDoubles.Default<MyPupilsPupilSelectionState>();

        Mock<IEvaluationHandler<UpdateMyPupilsSelectionStateRequest>> evaluationHandlerMock = new();

        // Arrange
        UpdateMyPupilsPupilSelectionsCommandHandler sut = new(
                providerMock.Object,
                selectionStateCommandHandler.Object,
                evaluationHandlerMock.Object);

        MyPupilsFormStateRequestDto request = new();

        // Act
        sut.Handle(request);

        // Assert
        providerMock.Verify(t => t.GetPupilSelections(), Times.Once);

        evaluationHandlerMock.Verify((handler)
            => handler.Evaluate(
                It.Is<UpdateMyPupilsSelectionStateRequest>(
                    (req) => ReferenceEquals(request, req.UpdateRequest) && ReferenceEquals(defaultState, req.State))),
                Times.Once);

        selectionStateCommandHandler.Verify(handler =>
            handler.StoreInSession(
                It.Is<MyPupilsPupilSelectionState>((store) => ReferenceEquals(defaultState, store))),
            Times.Once);
    }
}
