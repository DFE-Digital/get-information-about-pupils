using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.Options;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeleteAllPupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.Users.Application.Models;
using DfE.GIAP.SharedTests.Runtime.TestDoubles;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.MyPupils.Controllers;
using DfE.GIAP.Web.Features.MyPupils.Controllers.DeleteMyPupils;
using DfE.GIAP.Web.Features.MyPupils.Messaging;
using DfE.GIAP.Web.Features.MyPupils.PresentationService;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.GetPupilSelections;
using DfE.GIAP.Web.Shared.Session.Abstraction.Command;
using DfE.GIAP.Web.Tests.Controllers.Extensions;
using GraphQL.Types;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NSubstitute.Core;
using NuGet.Common;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.Controllers;
public sealed class DeleteMyPupilsControllerTests
{
    [Fact]
    public void Constructor_Throws_When_Logger_Is_Null()
    {
        // Arrange
        Func<DeleteMyPupilsController> construct = () => new(
            logger: null!,
            options: OptionsTestDoubles.Default<MyPupilsOptions>(),
            messagingOptions: OptionsTestDoubles.Default<MyPupilsMessagingOptions>(),
            messageSink: new Mock<IMyPupilsMessageSink>().Object,
            presentationService: new Mock<IMyPupilsPresentationService>().Object,
            pupilSelectionStateProvider: new Mock<IGetMyPupilsPupilSelectionProvider>().Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_Options_Is_Null()
    {
        // Arrange
        Func<DeleteMyPupilsController> construct = () => new(
            logger: new Mock<ILogger<DeleteMyPupilsController>>().Object,
            options: null!,
            messagingOptions: OptionsTestDoubles.Default<MyPupilsMessagingOptions>(),
            messageSink: new Mock<IMyPupilsMessageSink>().Object,
            presentationService: new Mock<IMyPupilsPresentationService>().Object,
            pupilSelectionStateProvider: new Mock<IGetMyPupilsPupilSelectionProvider>().Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_Options_Value_Is_Null()
    {
        // Arrange
        IOptions<MyPupilsOptions> nullValueOptions = OptionsTestDoubles.MockNullOptions<MyPupilsOptions>();

        Func<DeleteMyPupilsController> construct = () => new(
            logger: new Mock<ILogger<DeleteMyPupilsController>>().Object,
            options: nullValueOptions,
            messagingOptions: OptionsTestDoubles.Default<MyPupilsMessagingOptions>(),
            messageSink: new Mock<IMyPupilsMessageSink>().Object,
            presentationService: new Mock<IMyPupilsPresentationService>().Object,
            pupilSelectionStateProvider: new Mock<IGetMyPupilsPupilSelectionProvider>().Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_LoggingOptions_Is_Null()
    {
        // Arrange
        Func<DeleteMyPupilsController> construct = () => new(
            logger: new Mock<ILogger<DeleteMyPupilsController>>().Object,
            options: OptionsTestDoubles.Default<MyPupilsOptions>(),
            messagingOptions: null!,
            messageSink: new Mock<IMyPupilsMessageSink>().Object,
            presentationService: new Mock<IMyPupilsPresentationService>().Object,
            pupilSelectionStateProvider: new Mock<IGetMyPupilsPupilSelectionProvider>().Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_LoggingOptions_Value_Is_Null()
    {
        // Arrange
        IOptions<MyPupilsMessagingOptions> nullValueOptions = OptionsTestDoubles.MockNullOptions<MyPupilsMessagingOptions>();

        Func<DeleteMyPupilsController> construct = () => new(
            logger: new Mock<ILogger<DeleteMyPupilsController>>().Object,
            options: OptionsTestDoubles.Default<MyPupilsOptions>(),
            messagingOptions: nullValueOptions,
            messageSink: new Mock<IMyPupilsMessageSink>().Object,
            presentationService: new Mock<IMyPupilsPresentationService>().Object,
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
            options: OptionsTestDoubles.Default<MyPupilsOptions>(),
            messagingOptions: OptionsTestDoubles.Default<MyPupilsMessagingOptions>(),
            messageSink: null!,
            presentationService: new Mock<IMyPupilsPresentationService>().Object,
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
            options: OptionsTestDoubles.Default<MyPupilsOptions>(),
            messagingOptions: OptionsTestDoubles.Default<MyPupilsMessagingOptions>(),
            messageSink: new Mock<IMyPupilsMessageSink>().Object,
            presentationService: null!,
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
            options: OptionsTestDoubles.Default<MyPupilsOptions>(),
            messagingOptions: OptionsTestDoubles.Default<MyPupilsMessagingOptions>(),
            messageSink: new Mock<IMyPupilsMessageSink>().Object,
            presentationService: new Mock<IMyPupilsPresentationService>().Object,
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

        Mock<IMyPupilsPresentationService> presentationServiceMock = new();

        DeleteMyPupilsController sut = new(
            logger: loggerFake,
            messageSink: messageSinkMock.Object,
            presentationService: presentationServiceMock.Object,
            options: OptionsTestDoubles.Default<MyPupilsOptions>(),
            messagingOptions: OptionsTestDoubles.Default<MyPupilsMessagingOptions>(),
            pupilSelectionStateProvider: new Mock<IGetMyPupilsPupilSelectionProvider>().Object);

        sut.AddModelStateError();

        // Act
        IActionResult response = await sut.Delete([], It.IsAny<MyPupilsQueryRequestDto>());

        // Assert
        Assert.Equal("DeleteMyPupilsController.Delete POST method called", loggerFake.Logs.Single());
        ActionResultAssertionHelpers.AssertRedirectToGetMyPupils(response);

        messageSinkMock.Verify(
            (messageSink) => messageSink.AddMessage(
                It.Is<MyPupilsMessage>((message) =>
                    message.Level == MessageLevel.Error &&
                        message.Message == "There has been a problem with your delete selections. Please try again.")), Times.Once);

        presentationServiceMock.Verify((service)
            => service.DeletePupilsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [Fact]
    public async Task Delete_NoSelectionPupils_InRequest_Or_State_Adds_Error_And_Redirects_To_GetPupils()
    {
        // Arrange
        InMemoryLogger<DeleteMyPupilsController> loggerFake = LoggerTestDoubles.Fake<DeleteMyPupilsController>();

        Mock<IMyPupilsMessageSink> messageSinkMock = new();
        Mock<IMyPupilsPresentationService> presentationServiceMock = new();

        MyPupilsPupilSelectionState stateStub = MyPupilsPupilSelectionState.CreateDefault();

        Mock<IGetMyPupilsPupilSelectionProvider> providerMock = new();
        providerMock
            .Setup(provider => provider.GetPupilSelections())
            .Returns(stateStub);

            DeleteMyPupilsController sut = new(
            logger: loggerFake,
            messageSink: messageSinkMock.Object,
            presentationService: presentationServiceMock.Object,
            pupilSelectionStateProvider: providerMock.Object,
            options: OptionsTestDoubles.Default<MyPupilsOptions>(),
            messagingOptions: OptionsTestDoubles.Default<MyPupilsMessagingOptions>());

        // Act
        IActionResult response =
            await sut.Delete(
                SelectedPupils: [],
                It.IsAny<MyPupilsQueryRequestDto>());

        // Assert
        Assert.Equal("DeleteMyPupilsController.Delete POST method called", loggerFake.Logs.Single());
        ActionResultAssertionHelpers.AssertRedirectToGetMyPupils(response);

        providerMock.Verify(provider => provider.GetPupilSelections(), Times.Once);

        messageSinkMock.Verify(
            (messageSink) => messageSink.AddMessage(
                It.Is<MyPupilsMessage>((message) =>
                    message.Level == MessageLevel.Error &&
                        message.Message == "You have not selected any pupils")), Times.Once);

        presentationServiceMock.Verify((service)
            => service.DeletePupilsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [Fact]
    public async Task Delete_SomePupilsSelected_Deletes_SelectedPupils()
    {
        // Arrange
        IOptions<MyPupilsMessagingOptions> messagingOptionsStub = OptionsTestDoubles.Default<MyPupilsMessagingOptions>();
        InMemoryLogger<DeleteMyPupilsController> loggerFake = LoggerTestDoubles.Fake<DeleteMyPupilsController>();

        Mock<IMyPupilsMessageSink> messageSinkMock = new();
        Mock<IMyPupilsPresentationService> presentationServiceMock = new();

        MyPupilsPupilSelectionState stateStub = MyPupilsPupilSelectionState.CreateDefault();

        Mock<IGetMyPupilsPupilSelectionProvider> providerMock = new();
        providerMock
            .Setup(provider => provider.GetPupilSelections())
            .Returns(stateStub);

        DeleteMyPupilsController sut = new(
        logger: loggerFake,
        messageSink: messageSinkMock.Object,
        presentationService: presentationServiceMock.Object,
        pupilSelectionStateProvider: providerMock.Object,
        options: OptionsTestDoubles.Default<MyPupilsOptions>(),
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

        IActionResult response = await sut.Delete(selectedPupils, query);

        // Assert
        const string userIdStub = "00000000-0000-0000-0000-000000000000";

        Assert.Equal("DeleteMyPupilsController.Delete POST method called", loggerFake.Logs.Single());
        ActionResultAssertionHelpers.AssertRedirectToGetMyPupils(response, query);

        providerMock.Verify(provider => provider.GetPupilSelections(), Times.Once);

        messageSinkMock.Verify(
            (messageSink) => messageSink.AddMessage(
                It.Is<MyPupilsMessage>((message) =>
                    message.Id == messagingOptionsStub.Value.DeleteSuccessfulKey &&
                        message.Level == MessageLevel.Info &&
                            message.Message == $"Selected MyPupils were deleted from user: {userIdStub}.")), Times.Once);

        presentationServiceMock.Verify((service)
            => service.DeletePupilsAsync(
                It.Is<string>(userId => userId == userIdStub),
                It.Is<IEnumerable<string>>(t => t.SequenceEqual(selectedPupils))), Times.Once);
    }
}
