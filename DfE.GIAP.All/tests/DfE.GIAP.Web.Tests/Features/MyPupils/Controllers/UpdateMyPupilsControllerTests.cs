using DfE.GIAP.Web.Features.MyPupils.Controllers.UpdateForm;
using DfE.GIAP.Web.Features.MyPupils.Messaging;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.UpdatePupilSelections;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.Controllers;
public sealed class UpdateMyPupilsControllerTests
{
    [Fact]
    public void Constructor_Throws_When_Logger_Is_Null()
    {
        // Arrange
        Func<UpdateMyPupilsController> construct = () => new(
            null,
            new Mock<IMyPupilsMessageSink>().Object,
            new Mock<IUpdateMyPupilsPupilSelectionsCommandHandler>().Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }
}
