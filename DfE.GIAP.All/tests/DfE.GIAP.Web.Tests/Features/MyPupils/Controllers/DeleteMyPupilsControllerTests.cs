using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeleteAllPupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.SharedTests.TestDoubles;
using DfE.GIAP.Web.Features.MyPupils.Routes;
using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser;
using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser.ViewModels;
using DfE.GIAP.Web.Features.MyPupils.State;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;
using DfE.GIAP.Web.Features.MyPupils.ViewModel;
using DfE.GIAP.Web.Features.MyPupils.ViewModels.Factory;
using DfE.GIAP.Web.Session.Abstraction.Command;
using DfE.GIAP.Web.Tests.TestDoubles.MyPupils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.Controllers;
public sealed class DeleteMyPupilsControllerTests
{
    private const string MyPupilsViewPath = "~/Views/MyPupilList/Index.cshtml";

    [Fact]
    public void Constructor_Throws_When_Logger_Is_Null()
    {
        // Arrange
        Mock<ILogger<DeleteMyPupilsController>> loggerMock = new();
        Mock<IGetMyPupilsStateProvider> stateProviderMock = new();
        Mock<IGetPupilViewModelsHandler> handlerMock = new();
        Mock<IMyPupilsViewModelFactory> viewModelFactoryMock = new();
        Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> sessionCommandHandlerMock = new();
        Mock<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>> deletePupilsUseCaseMock = new();
        Mock<IUseCaseRequestOnly<DeleteAllMyPupilsRequest>> deleteAllPupilsUseCaseMock = new();

        // Act Assert
        Func<DeleteMyPupilsController> construct = () => new(
            logger: null,
            viewModelFactoryMock.Object,
            deleteAllPupilsUseCaseMock.Object,
            deletePupilsUseCaseMock.Object,
            stateProviderMock.Object,
            sessionCommandHandlerMock.Object,
            handlerMock.Object);

        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_ViewModelFactory_Is_Null()
    {
        // Arrange
        Mock<ILogger<DeleteMyPupilsController>> loggerMock = new();
        Mock<IGetMyPupilsStateProvider> stateProviderMock = new();
        Mock<IGetPupilViewModelsHandler> handlerMock = new();
        Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> sessionCommandHandlerMock = new();
        Mock<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>> deletePupilsUseCaseMock = new();
        Mock<IUseCaseRequestOnly<DeleteAllMyPupilsRequest>> deleteAllPupilsUseCaseMock = new();

        // Act Assert
        Func<DeleteMyPupilsController> construct = () => new(
            logger: loggerMock.Object,
            viewModelFactory: null,
            deleteAllPupilsUseCaseMock.Object,
            deletePupilsUseCaseMock.Object,
            stateProviderMock.Object,
            sessionCommandHandlerMock.Object,
            handlerMock.Object);

        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_DeleteAllPupilsUseCase_Is_Null()
    {
        // Arrange
        Mock<ILogger<DeleteMyPupilsController>> loggerMock = new();
        Mock<IGetMyPupilsStateProvider> stateProviderMock = new();
        Mock<IGetPupilViewModelsHandler> handlerMock = new();
        Mock<IMyPupilsViewModelFactory> viewModelFactoryMock = new();
        Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> sessionCommandHandlerMock = new();
        Mock<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>> deletePupilsUseCaseMock = new();
        Mock<IUseCaseRequestOnly<DeleteAllMyPupilsRequest>> deleteAllPupilsUseCaseMock = new();

        // Act Assert
        Func<DeleteMyPupilsController> construct = () => new(
            logger: loggerMock.Object,
            viewModelFactoryMock.Object,
            deleteAllPupilsUseCase: null,
            deletePupilsUseCaseMock.Object,
            stateProviderMock.Object,
            sessionCommandHandlerMock.Object,
            handlerMock.Object);

        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_DeletePupilsUseCase_Is_Null()
    {
        // Arrange
        Mock<ILogger<DeleteMyPupilsController>> loggerMock = new();
        Mock<IGetMyPupilsStateProvider> stateProviderMock = new();
        Mock<IGetPupilViewModelsHandler> handlerMock = new();
        Mock<IMyPupilsViewModelFactory> viewModelFactoryMock = new();
        Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> sessionCommandHandlerMock = new();
        Mock<IUseCaseRequestOnly<DeleteAllMyPupilsRequest>> deleteAllPupilsUseCaseMock = new();

        // Act Assert
        Func<DeleteMyPupilsController> construct = () => new(
            logger: loggerMock.Object,
            viewModelFactoryMock.Object,
            deleteAllPupilsUseCase: deleteAllPupilsUseCaseMock.Object,
            deleteSomePupilsUseCase: null,
            stateProviderMock.Object,
            sessionCommandHandlerMock.Object,
            handlerMock.Object);

        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_SessionCommandHandler_Is_Null()
    {
        // Arrange
        Mock<ILogger<DeleteMyPupilsController>> loggerMock = new();
        Mock<IGetMyPupilsStateProvider> stateProviderMock = new();
        Mock<IGetPupilViewModelsHandler> handlerMock = new();
        Mock<IMyPupilsViewModelFactory> viewModelFactoryMock = new();
        Mock<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>> deletePupilsUseCaseMock = new();
        Mock<IUseCaseRequestOnly<DeleteAllMyPupilsRequest>> deleteAllPupilsUseCaseMock = new();

        // Act Assert
        Func<DeleteMyPupilsController> construct = () => new(
            logger: null,
            viewModelFactoryMock.Object,
            deleteAllPupilsUseCaseMock.Object,
            deletePupilsUseCaseMock.Object,
            stateProviderMock.Object,
            selectionStateSessionCommandHandler: null,
            handlerMock.Object);

        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_GetPupilViewModelsHandler_Is_Null()
    {
        // Arrange
        Mock<ILogger<DeleteMyPupilsController>> loggerMock = new();
        Mock<IGetMyPupilsStateProvider> stateProviderMock = new();
        Mock<IGetPupilViewModelsHandler> handlerMock = new();
        Mock<IMyPupilsViewModelFactory> viewModelFactoryMock = new();
        Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> sessionCommandHandlerMock = new();
        Mock<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>> deletePupilsUseCaseMock = new();
        Mock<IUseCaseRequestOnly<DeleteAllMyPupilsRequest>> deleteAllPupilsUseCaseMock = new();

        // Act Assert
        Func<DeleteMyPupilsController> construct = () => new(
            logger: null,
            viewModelFactoryMock.Object,
            deleteAllPupilsUseCaseMock.Object,
            deletePupilsUseCaseMock.Object,
            stateProviderMock.Object,
            sessionCommandHandlerMock.Object,
            getPupilViewModelsHandler: null);

        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task Delete_ModelStateIsInvalid_Returns_ViewModelError_Without_UseCase_Call()
    {
        // Arrange
        InMemoryLogger<DeleteMyPupilsController> loggerMock = LoggerTestDoubles.MockLogger<DeleteMyPupilsController>();
        Mock<IGetMyPupilsStateProvider> stateProviderMock = new();
        Mock<IGetPupilViewModelsHandler> handlerMock = new();
        Mock<IMyPupilsViewModelFactory> viewModelFactoryMock = new();
        Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> sessionCommandHandlerMock = new();
        Mock<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>> deletePupilsUseCaseMock = new();
        Mock<IUseCaseRequestOnly<DeleteAllMyPupilsRequest>> deleteAllPupilsUseCaseMock = new();

        stateProviderMock
            .Setup(t => t.GetState())
            .Returns(MyPupilsStateTestDoubles.Default())
            .Verifiable();

        viewModelFactoryMock.Setup(
            (factory)
                => factory.CreateViewModel(
                    It.IsAny<MyPupilsState>(),
                    It.IsAny<PupilsViewModel>(),
                    It.IsAny<MyPupilsViewModelContext>()))
                .Returns(new MyPupilsViewModel(pupils: PupilsViewModelTestDoubles.Generate(10)))
                .Verifiable();

        DeleteMyPupilsController sut = new(
            loggerMock,
            viewModelFactoryMock.Object,
            deleteAllPupilsUseCaseMock.Object,
            deletePupilsUseCaseMock.Object,
            stateProviderMock.Object,
            sessionCommandHandlerMock.Object,
            handlerMock.Object);

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
        Assert.NotNull(myPupilsViewModel.Pupils);
        Assert.Equal(10, myPupilsViewModel.Pupils.Count);

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
                It.IsAny<PupilsViewModel>(),
                It.Is<MyPupilsViewModelContext>(t => t.Error.Equals("There has been a problem with selections. Please try again."))), Times.Once);
    }

    [Fact]
    public async Task Delete_NoSelectedPupils_Returns_ViewModelError_Without_UseCase_Call()
    {
        // Arrange
        InMemoryLogger<DeleteMyPupilsController> loggerMock = LoggerTestDoubles.MockLogger<DeleteMyPupilsController>();
        Mock<IGetMyPupilsStateProvider> stateProviderMock = new();
        Mock<IGetPupilViewModelsHandler> handlerMock = new();
        Mock<IMyPupilsViewModelFactory> viewModelFactoryMock = new();
        Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> sessionCommandHandlerMock = new();
        Mock<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>> deletePupilsUseCaseMock = new();
        Mock<IUseCaseRequestOnly<DeleteAllMyPupilsRequest>> deleteAllPupilsUseCaseMock = new();

        MyPupilsPupilSelectionState noPupilsSelectedInState = MyPupilsPupilSelectionStateTestDoubles.WithPupilsSelectionState([]);

        stateProviderMock
            .Setup(t => t.GetState())
            .Returns(
                MyPupilsStateTestDoubles.Create(
                    MyPupilsPresentationStateTestDoubles.Default(),
                    noPupilsSelectedInState))
            .Verifiable();

        viewModelFactoryMock.Setup(
            (factory)
                => factory.CreateViewModel(
                    It.IsAny<MyPupilsState>(),
                    It.IsAny<PupilsViewModel>(),
                    It.IsAny<MyPupilsViewModelContext>()))
                .Returns(new MyPupilsViewModel(pupils: PupilsViewModelTestDoubles.Generate(10)))
                .Verifiable();

        DeleteMyPupilsController sut = new(
            loggerMock,
            viewModelFactoryMock.Object,
            deleteAllPupilsUseCaseMock.Object,
            deletePupilsUseCaseMock.Object,
            stateProviderMock.Object,
            sessionCommandHandlerMock.Object,
            handlerMock.Object);

        sut.StubHttpContext();

        // Act
        IActionResult result = await sut.Delete(SelectedPupils: []);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        Assert.Equal(MyPupilsViewPath, viewResult.ViewName);

        MyPupilsViewModel myPupilsViewModel = Assert.IsType<MyPupilsViewModel>(viewResult.Model);
        Assert.NotNull(myPupilsViewModel);
        Assert.NotNull(myPupilsViewModel.Pupils);
        Assert.Equal(10, myPupilsViewModel.Pupils.Count);

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
                It.IsAny<PupilsViewModel>(),
                It.Is<MyPupilsViewModelContext>(t => t.Error.Equals("You have not selected any pupils"))), Times.Once);
    }
}
