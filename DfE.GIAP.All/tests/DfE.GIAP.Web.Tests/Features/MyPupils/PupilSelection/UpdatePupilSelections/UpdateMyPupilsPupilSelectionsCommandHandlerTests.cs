using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.CommandHandlers;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.UpdatePupilSelections;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.GetPupilSelections;
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
}
