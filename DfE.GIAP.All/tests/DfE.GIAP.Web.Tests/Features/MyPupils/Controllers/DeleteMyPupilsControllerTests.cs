using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.Options;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeleteAllPupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.SharedTests.Runtime.TestDoubles;
using DfE.GIAP.Web.Features.MyPupils.Controllers;
using DfE.GIAP.Web.Features.MyPupils.Controllers.DeleteMyPupils;
using DfE.GIAP.Web.Features.MyPupils.Messaging;
using DfE.GIAP.Web.Features.MyPupils.PresentationService;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.GetPupilSelections;
using DfE.GIAP.Web.Shared.Session.Abstraction.Command;
using DfE.GIAP.Web.Tests.Controllers.Extensions;
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
            myPupilsMessageSink: new Mock<IMyPupilsMessageSink>().Object,
            myPupilsPresentationService: new Mock<IMyPupilsPresentationService>().Object,
            getMyPupilsStateProvider: new Mock<IGetMyPupilsPupilSelectionProvider>().Object);

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
            myPupilsMessageSink: new Mock<IMyPupilsMessageSink>().Object,
            myPupilsPresentationService: new Mock<IMyPupilsPresentationService>().Object,
            getMyPupilsStateProvider: new Mock<IGetMyPupilsPupilSelectionProvider>().Object);

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
            myPupilsMessageSink: new Mock<IMyPupilsMessageSink>().Object,
            myPupilsPresentationService: new Mock<IMyPupilsPresentationService>().Object,
            getMyPupilsStateProvider: new Mock<IGetMyPupilsPupilSelectionProvider>().Object);

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
            myPupilsMessageSink: new Mock<IMyPupilsMessageSink>().Object,
            myPupilsPresentationService: new Mock<IMyPupilsPresentationService>().Object,
            getMyPupilsStateProvider: new Mock<IGetMyPupilsPupilSelectionProvider>().Object);

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
            myPupilsMessageSink: new Mock<IMyPupilsMessageSink>().Object,
            myPupilsPresentationService: new Mock<IMyPupilsPresentationService>().Object,
            getMyPupilsStateProvider: new Mock<IGetMyPupilsPupilSelectionProvider>().Object);

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
            myPupilsMessageSink: null!,
            myPupilsPresentationService: new Mock<IMyPupilsPresentationService>().Object,
            getMyPupilsStateProvider: new Mock<IGetMyPupilsPupilSelectionProvider>().Object);

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
            myPupilsMessageSink: new Mock<IMyPupilsMessageSink>().Object,
            myPupilsPresentationService: null!,
            getMyPupilsStateProvider: new Mock<IGetMyPupilsPupilSelectionProvider>().Object);

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
            myPupilsMessageSink: new Mock<IMyPupilsMessageSink>().Object,
            myPupilsPresentationService: new Mock<IMyPupilsPresentationService>().Object,
            getMyPupilsStateProvider: null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    
    [Fact]
    public async Task Delete_When_ModelState_Invalid_Adds_Error_And_Redirects_To_GetPupils()
    {
        // Arrange
        InMemoryLogger<DeleteMyPupilsController> loggerFake = LoggerTestDoubles.Fake<DeleteMyPupilsController>();

        Mock<IMyPupilsMessageSink> messageSinkMock = new(); 

        DeleteMyPupilsController sut = new(
            logger: loggerFake,
            myPupilsMessageSink: messageSinkMock.Object,
            options: OptionsTestDoubles.Default<MyPupilsOptions>(),
            messagingOptions: OptionsTestDoubles.Default<MyPupilsMessagingOptions>(),
            myPupilsPresentationService: new Mock<IMyPupilsPresentationService>().Object,
            getMyPupilsStateProvider: new Mock<IGetMyPupilsPupilSelectionProvider>().Object);

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
    }

/*

[Fact]
        public async Task Delete_ModelStateIsInvalid_Returns_ViewModelError_Without_UseCase_Call()
        {
            // Arrange
            InMemoryLogger<DeleteMyPupilsController> loggerMock = LoggerTestDoubles.MockLogger<DeleteMyPupilsController>();
            Mock<IGetMyPupilsPupilSelectionProvider> stateProviderMock = new();
            Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> sessionCommandHandlerMock = new();
            Mock<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>> deletePupilsUseCaseMock = new();
            Mock<IUseCaseRequestOnly<DeleteAllMyPupilsRequest>> deleteAllPupilsUseCaseMock = new();

            stateProviderMock
                .Setup(t => t.GetPupilSelections())
                .Returns(MyPupilsStateTestDoubles.Default())
                .Verifiable();

            viewModelFactoryMock.Setup(
                (factory)
                    => factory.CreateViewModel(
                        It.IsAny<MyPupilsState>(),
                        It.IsAny<MyPupilsPresentationPupilModels>(),
                        It.IsAny<MyPupilsViewModelContext>()))
                    .Returns(new MyPupilsViewModel(
                        pupils: MyPupilsPresentationModelTestDoubles.Generate(10)))
                    .Verifiable();

            DeleteMyPupilsController sut = new(
                loggerMock,
                deleteAllPupilsUseCaseMock.Object,
                deletePupilsUseCaseMock.Object,
                stateProviderMock.Object,
                sessionCommandHandlerMock.Object);

            sut.StubHttpContext();
            sut.ModelState.AddModelError("any", "error");

            // Act
            IActionResult result = await sut.Delete(It.IsAny<List<string>>());

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            Assert.Equal(MyPupilsViewPath, viewResult.ViewName);

            MyPupilsViewModel myPupilsViewModel = Assert.IsType<MyPupilsViewModel>(viewResult.Model);
            Assert.NotNull(myPupilsViewModel);
            Assert.NotNull(myPupilsViewModel.CurrentPageOfPupils);
            Assert.Equal(10, myPupilsViewModel.CurrentPageOfPupils.Count);

            string log = Assert.Single(loggerMock.Logs);
            Assert.Equal("DeleteMyPupilsController.Delete POST method called", log);

            sessionCommandHandlerMock.Verify(
                (handler) => handler.StoreInSession(It.IsAny<MyPupilsPupilSelectionState>()), Times.Never);

            deletePupilsUseCaseMock.Verify(
                (useCase) => useCase.HandleRequestAsync(It.IsAny<DeletePupilsFromMyPupilsRequest>()), Times.Never);

            deleteAllPupilsUseCaseMock.Verify(
                (useCase) => useCase.HandleRequestAsync(It.IsAny<DeleteAllMyPupilsRequest>()), Times.Never);

            viewModelFactoryMock.Verify(
                (viewModelFactory) => viewModelFactory.CreateViewModel(
                    It.IsAny<MyPupilsState>(),
                    It.IsAny<MyPupilsPresentationPupilModels>(),
                    It.Is<MyPupilsViewModelContext>(t => t.Error.Equals("There has been a problem with selections. Please try again."))), Times.Once);
        }

        [Fact]
        public async Task Delete_NoSelectedPupils_Returns_ViewModelError_Without_UseCase_Call()
        {
            // Arrange
            InMemoryLogger<DeleteMyPupilsController> loggerMock = LoggerTestDoubles.MockLogger<DeleteMyPupilsController>();
            Mock<IGetMyPupilsPupilSelectionProvider> stateProviderMock = new();
            Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> sessionCommandHandlerMock = new();
            Mock<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>> deletePupilsUseCaseMock = new();
            Mock<IUseCaseRequestOnly<DeleteAllMyPupilsRequest>> deleteAllPupilsUseCaseMock = new();

            MyPupilsPupilSelectionState noPupilsSelectedInState = MyPupilsPupilSelectionStateTestDoubles.WithPupilsSelectionState([]);

            stateProviderMock
                .Setup(t => t.GetPupilSelections())
                .Returns(
                    MyPupilsStateTestDoubles.Create(
                        MyPupilsPresentationQueyTestDoubles.Default(),
                        noPupilsSelectedInState))
                .Verifiable();

            viewModelFactoryMock.Setup(
                (factory)
                    => factory.CreateViewModel(
                        It.IsAny<MyPupilsState>(),
                        It.IsAny<MyPupilsPresentationPupilModels>(),
                        It.IsAny<MyPupilsViewModelContext>()))
                    .Returns(new MyPupilsViewModel(pupils: MyPupilsPresentationModelTestDoubles.Generate(10)))
                    .Verifiable();

            DeleteMyPupilsController sut = new(
                loggerMock,
                deleteAllPupilsUseCaseMock.Object,
                deletePupilsUseCaseMock.Object,
                stateProviderMock.Object,
                sessionCommandHandlerMock.Object);

            sut.StubHttpContext();

            // Act
            IActionResult result = await sut.Delete(SelectedPupils: []);

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            Assert.Equal(MyPupilsViewPath, viewResult.ViewName);

            MyPupilsViewModel myPupilsViewModel = Assert.IsType<MyPupilsViewModel>(viewResult.Model);
            Assert.NotNull(myPupilsViewModel);
            Assert.NotNull(myPupilsViewModel.CurrentPageOfPupils);
            Assert.Equal(10, myPupilsViewModel.CurrentPageOfPupils.Count);

            string log = Assert.Single(loggerMock.Logs);
            Assert.Equal("DeleteMyPupilsController.Delete POST method called", log);

            sessionCommandHandlerMock.Verify(
                (handler) => handler.StoreInSession(It.IsAny<MyPupilsPupilSelectionState>()), Times.Never);

            deletePupilsUseCaseMock.Verify(
                (useCase) => useCase.HandleRequestAsync(It.IsAny<DeletePupilsFromMyPupilsRequest>()), Times.Never);

            deleteAllPupilsUseCaseMock.Verify(
                (useCase) => useCase.HandleRequestAsync(It.IsAny<DeleteAllMyPupilsRequest>()), Times.Never);

            viewModelFactoryMock.Verify(
                (viewModelFactory) => viewModelFactory.CreateViewModel(
                    It.IsAny<MyPupilsState>(),
                    It.IsAny<MyPupilsPresentationPupilModels>(),
                    It.Is<MyPupilsViewModelContext>(t => t.Error.Equals("You have not selected any pupils"))), Times.Once);
        }

        [Fact]
        public async Task Delete_AllPupils_DeletesAllPupils()
        {
            // Arrange
            InMemoryLogger<DeleteMyPupilsController> loggerMock = LoggerTestDoubles.MockLogger<DeleteMyPupilsController>();
            Mock<IGetMyPupilsPupilSelectionProvider> stateProviderMock = new();
            Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> sessionCommandHandlerMock = new();
            Mock<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>> deletePupilsUseCaseMock = new();
            Mock<IUseCaseRequestOnly<DeleteAllMyPupilsRequest>> deleteAllPupilsUseCaseMock = new();

            MyPupilsPupilSelectionState allPupilsSelectedStub = MyPupilsPupilSelectionStateTestDoubles.WithAllPupilsSelected([]);

            stateProviderMock
                .Setup(t => t.GetPupilSelections())
                .Returns(
                    MyPupilsStateTestDoubles.Create(
                        MyPupilsPresentationQueyTestDoubles.Default(),
                        allPupilsSelectedStub))
                .Verifiable();

            MyPupilsPresentationPupilModels pupilsOnPage = MyPupilsPresentationModelTestDoubles.Generate(10);
            MyPupilsResponse response = new(pupilsOnPage);

            getPupilsHandlerMock
                .Setup(t => t.GetPupilsAsync(It.IsAny<MyPupilsRequest>()))
                .ReturnsAsync(response)
                .Verifiable();

            DeleteMyPupilsController sut = new(
                loggerMock,
                deleteAllPupilsUseCaseMock.Object,
                deletePupilsUseCaseMock.Object,
                stateProviderMock.Object,
                sessionCommandHandlerMock.Object);

            Dictionary<string, object?> tempDataDictionaryStub = [];
            HttpContext stubbedHttpContext = sut.StubHttpContext();
            sut.StubTempData(tempDataDictionaryStub, stubbedHttpContext);

            const string stubbedClaimsPrincipalCustomUserIdClaim = "00000000-0000-0000-0000-000000000000";

            // Act
            IActionResult result = await sut.Delete(
                SelectedPupils: pupilsOnPage.Values.Select(
                    (pupil) => pupil.UniquePupilNumber).ToList());

            // Assert
            RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.NotNull(redirectResult);
            Assert.Equal(nameof(GetMyPupilsController).Replace("Controller", string.Empty), redirectResult.ControllerName);
            Assert.Equal(nameof(GetMyPupilsController.Index), redirectResult.ActionName);

            // Assert logs
            string log = Assert.Single(loggerMock.Logs);
            Assert.Equal("DeleteMyPupilsController.Delete POST method called", log);

            // Assert state is reset, delete all is dispatched
            Assert.False(allPupilsSelectedStub.IsAnyPupilSelected);
            Assert.False(allPupilsSelectedStub.IsAllPupilsSelected);
            Assert.Empty(allPupilsSelectedStub.GetSelectedPupils());
            deleteAllPupilsUseCaseMock.Verify(
                (useCase) => useCase.HandleRequestAsync(It.Is<DeleteAllMyPupilsRequest>(request => request.UserId == stubbedClaimsPrincipalCustomUserIdClaim)), Times.Once);

            sessionCommandHandlerMock.Verify(
                (handler) => handler.StoreInSession(allPupilsSelectedStub), Times.Once);

    #pragma warning disable CS8605 // Unboxing a possibly null value.
            Assert.True((bool)sut.TempData["IsDeleteSuccessful"]);
    #pragma warning restore CS8605 // Unboxing a possibly null value.

            deletePupilsUseCaseMock.Verify(
                (useCase) => useCase.HandleRequestAsync(It.IsAny<DeletePupilsFromMyPupilsRequest>()), Times.Never);
        }
    }
    */
}
