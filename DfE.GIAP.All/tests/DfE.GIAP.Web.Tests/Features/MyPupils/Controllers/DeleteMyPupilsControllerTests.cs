using DfE.GIAP.SharedTests.Runtime.TestDoubles;
using DfE.GIAP.Web.Features.MyPupils.Controllers;
using DfE.GIAP.Web.Features.MyPupils.Controllers.DeleteMyPupils;
using DfE.GIAP.Web.Features.MyPupils.Messaging;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.GetPupilSelections;
using DfE.GIAP.Web.Features.MyPupils.Services.DeletePupils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.Controllers;
public sealed class DeleteMyPupilsControllerTests
{
    [Fact]
    public void Constructor_Throws_When_Logger_Is_Null()
    {
        // Arrange
        Func<DeleteMyPupilsController> construct = () => new(
            logger: null!,
            messagingOptions: OptionsTestDoubles.Default<MyPupilsMessagingOptions>(),
            messageSink: new Mock<IMyPupilsMessageSink>().Object,
            deleteService: new Mock<IDeleteMyPupilsPresentationService>().Object,
            pupilSelectionStateProvider: new Mock<IGetMyPupilsPupilSelectionProvider>().Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_MessagingOptions_Is_Null()
    {
        // Arrange
        Func<DeleteMyPupilsController> construct = () => new(
            logger: new Mock<ILogger<DeleteMyPupilsController>>().Object,
            messagingOptions: null!,
            messageSink: new Mock<IMyPupilsMessageSink>().Object,
            deleteService: new Mock<IDeleteMyPupilsPresentationService>().Object,
            pupilSelectionStateProvider: new Mock<IGetMyPupilsPupilSelectionProvider>().Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_MessagingOptions_Value_Is_Null()
    {
        // Arrange
        IOptions<MyPupilsMessagingOptions> nullValueOptions = OptionsTestDoubles.MockNullOptions<MyPupilsMessagingOptions>();

        Func<DeleteMyPupilsController> construct = () => new(
            logger: new Mock<ILogger<DeleteMyPupilsController>>().Object,
            messagingOptions: nullValueOptions,
            messageSink: new Mock<IMyPupilsMessageSink>().Object,
            deleteService: new Mock<IDeleteMyPupilsPresentationService>().Object,
            pupilSelectionStateProvider: new Mock<IGetMyPupilsPupilSelectionProvider>().Object);

        // Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_MessageSink_Is_Null()
    {
        // Arrange
        Func<DeleteMyPupilsController> construct = () => new(
            logger: new Mock<ILogger<DeleteMyPupilsController>>().Object,
            messagingOptions: OptionsTestDoubles.Default<MyPupilsMessagingOptions>(),
            messageSink: null!,
            deleteService: new Mock<IDeleteMyPupilsPresentationService>().Object,
            pupilSelectionStateProvider: new Mock<IGetMyPupilsPupilSelectionProvider>().Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_PresentationService_Is_Null()
    {
        // Arrange
        Func<DeleteMyPupilsController> construct = () => new(
            logger: new Mock<ILogger<DeleteMyPupilsController>>().Object,
            messagingOptions: OptionsTestDoubles.Default<MyPupilsMessagingOptions>(),
            messageSink: new Mock<IMyPupilsMessageSink>().Object,
            deleteService: null!,
            pupilSelectionStateProvider: new Mock<IGetMyPupilsPupilSelectionProvider>().Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_StateProvider_Is_Null()
    {
        // Arrange
        Func<DeleteMyPupilsController> construct = () => new(
            logger: new Mock<ILogger<DeleteMyPupilsController>>().Object,
            messagingOptions: OptionsTestDoubles.Default<MyPupilsMessagingOptions>(),
            messageSink: new Mock<IMyPupilsMessageSink>().Object,
            deleteService: new Mock<IDeleteMyPupilsPresentationService>().Object,
            pupilSelectionStateProvider: null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task Delete_When_ModelState_Invalid_Adds_Error_And_Redirects_To_GetPupils()
    {
        // Arrange
        InMemoryLogger<DeleteMyPupilsController> loggerFake = LoggerTestDoubles.Fake<DeleteMyPupilsController>();

        Mock<IMyPupilsMessageSink> messageSinkMock = new();

        Mock<IDeleteMyPupilsPresentationService> deleteServiceMock = new();

        DeleteMyPupilsController sut = new(
            logger: loggerFake,
            messageSink: messageSinkMock.Object,
            deleteService: deleteServiceMock.Object,
            messagingOptions: OptionsTestDoubles.Default<MyPupilsMessagingOptions>(),
            pupilSelectionStateProvider: new Mock<IGetMyPupilsPupilSelectionProvider>().Object);

        sut.AddModelStateError();

        // Act
        IActionResult response = await sut.Index([], It.IsAny<MyPupilsQueryRequestDto>());

        // Assert
        Assert.Equal("DeleteMyPupilsController.Index POST method called", loggerFake.Logs.Single());
        ActionResultAssertionHelpers.AssertRedirectToGetMyPupils(response);

        messageSinkMock.Verify(
            (messageSink) => messageSink.AddMessage(
                It.Is<MyPupilsMessage>((message) =>
                    message.Level == MessageLevel.Error &&
                        message.Message == "There has been a problem with your delete selections. Please try again.")), Times.Once);

        deleteServiceMock.Verify((service)
            => service.DeletePupilsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [Fact]
    public async Task Delete_NoSelectionPupils_InRequest_Or_State_Adds_Error_And_Redirects_To_GetPupils()
    {
        // Arrange
        InMemoryLogger<DeleteMyPupilsController> loggerFake = LoggerTestDoubles.Fake<DeleteMyPupilsController>();

        Mock<IMyPupilsMessageSink> messageSinkMock = new();
        Mock<IDeleteMyPupilsPresentationService> deleteServiceMock = new();

        MyPupilsPupilSelectionState stateStub = MyPupilsPupilSelectionState.CreateDefault();

        Mock<IGetMyPupilsPupilSelectionProvider> providerMock = new();
        providerMock
            .Setup(provider => provider.GetPupilSelections())
            .Returns(stateStub);

            DeleteMyPupilsController sut = new(
            logger: loggerFake,
            messageSink: messageSinkMock.Object,
            deleteService: deleteServiceMock.Object,
            pupilSelectionStateProvider: providerMock.Object,
            messagingOptions: OptionsTestDoubles.Default<MyPupilsMessagingOptions>());

        // Act
        IActionResult response =
            await sut.Index(
                SelectedPupils: [],
                It.IsAny<MyPupilsQueryRequestDto>());

        // Assert
        Assert.Equal("DeleteMyPupilsController.Index POST method called", loggerFake.Logs.Single());
        ActionResultAssertionHelpers.AssertRedirectToGetMyPupils(response);

        providerMock.Verify(provider => provider.GetPupilSelections(), Times.Once);

        messageSinkMock.Verify(
            (messageSink) => messageSink.AddMessage(
                It.Is<MyPupilsMessage>((message) =>
                    message.Level == MessageLevel.Error &&
                        message.Message == "You have not selected any pupils")), Times.Once);

        deleteServiceMock.Verify((service)
            => service.DeletePupilsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [Fact]
    public async Task Delete_SomePupilsSelected_Deletes_SelectedPupils()
    {
        // Arrange
        IOptions<MyPupilsMessagingOptions> messagingOptionsStub = OptionsTestDoubles.Default<MyPupilsMessagingOptions>();
        InMemoryLogger<DeleteMyPupilsController> loggerFake = LoggerTestDoubles.Fake<DeleteMyPupilsController>();

        Mock<IMyPupilsMessageSink> messageSinkMock = new();
        Mock<IDeleteMyPupilsPresentationService> deleteServiceMock = new();

        MyPupilsPupilSelectionState stateStub = MyPupilsPupilSelectionState.CreateDefault();

        Mock<IGetMyPupilsPupilSelectionProvider> providerMock = new();
        providerMock
            .Setup(provider => provider.GetPupilSelections())
            .Returns(stateStub);

        DeleteMyPupilsController sut = new(
        logger: loggerFake,
        messageSink: messageSinkMock.Object,
        deleteService: deleteServiceMock.Object,
        pupilSelectionStateProvider: providerMock.Object,
        messagingOptions: messagingOptionsStub);

        HttpContext httpContextStub = sut.StubHttpContext();

        // Act
        MyPupilsQueryRequestDto query = new()
        {
            PageNumber = 2,
            SortField = "name",
            SortDirection = "asc"
        };
        List<string> selectedPupils = ["a"];

        IActionResult response = await sut.Index(selectedPupils, query);

        // Assert
        const string userIdStub = "00000000-0000-0000-0000-000000000000";

        Assert.Equal("DeleteMyPupilsController.Index POST method called", loggerFake.Logs.Single());
        ActionResultAssertionHelpers.AssertRedirectToGetMyPupils(response, query);

        providerMock.Verify(provider => provider.GetPupilSelections(), Times.Once);

        messageSinkMock.Verify(
            (messageSink) => messageSink.AddMessage(
                It.Is<MyPupilsMessage>((message) =>
                    message.Id == messagingOptionsStub.Value.DeleteSuccessfulKey &&
                        message.Level == MessageLevel.Info &&
                            message.Message == $"Selected MyPupils were deleted from user: {userIdStub}.")), Times.Once);

        deleteServiceMock.Verify((service)
            => service.DeletePupilsAsync(
                It.Is<string>(userId => userId == userIdStub),
                It.Is<IEnumerable<string>>(t => t.SequenceEqual(selectedPupils))), Times.Once);
    }
}
