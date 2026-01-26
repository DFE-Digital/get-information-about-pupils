using DfE.GIAP.SharedTests.Runtime.TestDoubles;
using DfE.GIAP.Web.Features.MyPupils.Areas.UpdateForm;
using DfE.GIAP.Web.Features.MyPupils.Controllers;
using DfE.GIAP.Web.Features.MyPupils.Controllers.UpdateForm;
using DfE.GIAP.Web.Features.MyPupils.Messaging;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.UpdatePupilSelections;
using Microsoft.AspNetCore.Mvc;
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
            null!,
            new Mock<IMyPupilsMessageSink>().Object,
            new Mock<IUpdateMyPupilsPupilSelectionsCommandHandler>().Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_MessageSink_Is_Null()
    {
        // Arrange
        Func<UpdateMyPupilsController> construct = () => new(
            LoggerTestDoubles.Fake<UpdateMyPupilsController>(),
            null!,
            new Mock<IUpdateMyPupilsPupilSelectionsCommandHandler>().Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_Handler_Is_Null()
    {
        // Arrange
        Func<UpdateMyPupilsController> construct = () => new(
            LoggerTestDoubles.Fake<UpdateMyPupilsController>(),
            new Mock<IMyPupilsMessageSink>().Object,
            null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Index_Adds_Error_When_ModelStateInvalid_And_Redirects_To_GetMyPupils()
    {
        // Arrange
        InMemoryLogger<UpdateMyPupilsController> loggerFake = LoggerTestDoubles.Fake<UpdateMyPupilsController>();

        Mock<IMyPupilsMessageSink> messageSinkMock = new();

        Mock<IUpdateMyPupilsPupilSelectionsCommandHandler> handlerMock = new();

        UpdateMyPupilsController sut = new(
            loggerFake,
            messageSinkMock.Object,
            handlerMock.Object
        );

        // Act
        IActionResult response = sut.Index(null, null);

        // Assert
        Assert.Equal("UpdateMyPupilsController.Index POST method called", loggerFake.Logs.Single());
        ActionResultAssertionHelpers.AssertRedirectToGetMyPupils(response);

        handlerMock.Verify(t => t.Handle(It.IsAny<MyPupilsFormStateRequestDto>()), Times.Never);
    }

    [Fact]
    public void Index_Calls_Handler_And_Redirects_To_GetMyPupils()
    {
        // Arrange
        InMemoryLogger<UpdateMyPupilsController> loggerFake = LoggerTestDoubles.Fake<UpdateMyPupilsController>();

        Mock<IMyPupilsMessageSink> messageSinkMock = new();

        Mock<IUpdateMyPupilsPupilSelectionsCommandHandler> handlerMock = new();

        UpdateMyPupilsController sut = new(
            loggerFake,
            messageSinkMock.Object,
            handlerMock.Object
        );

        // Act
        MyPupilsQueryRequestDto query = new()
        {
            PageNumber = 2,
            SortDirection = "asc",
            SortField = "any_field"
        };

        MyPupilsFormStateRequestDto request = new();

        IActionResult response = sut.Index(request, query);

        // Assert
        Assert.Equal("UpdateMyPupilsController.Index POST method called", loggerFake.Logs.Single());
        ActionResultAssertionHelpers.AssertRedirectToGetMyPupils(response, query);

        handlerMock.Verify((handler)
            => handler.Handle(
                It.Is<MyPupilsFormStateRequestDto>(
                    (req) => ReferenceEquals(request, req))), Times.Once);

        messageSinkMock.Verify(t => t.AddMessage(It.IsAny<MyPupilsMessage>()), Times.Never);
    }
}
