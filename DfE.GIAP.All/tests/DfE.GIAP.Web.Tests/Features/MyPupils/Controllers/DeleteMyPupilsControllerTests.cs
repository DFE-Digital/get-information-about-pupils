using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeleteAllPupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Web.Features.MyPupils.Routes;
using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser;
using DfE.GIAP.Web.Features.MyPupils.State;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;
using DfE.GIAP.Web.Features.MyPupils.ViewModel;
using DfE.GIAP.Web.Session.Abstraction.Command;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.Controllers;
public sealed class DeleteMyPupilsControllerTests
{
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
            myPupilsViewModelFactory: null,
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
    public async Task Delete_ReturnsView_WhenModelStateIsInvalid()
    {
        //// Arrange
        //Mock<IMyPupilsViewModelFactory> viewModelFactoryMock = new();
        //Mock<IGetMyPupilsStateProvider> stateProviderMock = new();
        //Mock<IGetPupilViewModelsHandler> handlerMock = new();
        //Mock<ILogger<GetMyPupilsController>> loggerMock = new();

        //GetMyPupilsController controller = new(
        //    loggerMock.Object,
        //    viewModelFactoryMock.Object,
        //    stateProviderMock.Object,
        //    handlerMock.Object);

        //controller.ModelState.AddModelError("SelectedPupils", "Required");

        //List<string> selectedPupils = new(); // or null

        //MyPupilsViewModel expectedViewModel = new(new PupilsViewModel());
        //viewModelFactoryMock
        //    .Setup(factory => factory.CreateViewModel(
        //        It.IsAny<MyPupilsState>(),
        //        It.IsAny<PupilsViewModel>(),
        //        It.IsAny<MyPupilsErrorViewModel?>(),
        //        It.IsAny<bool>()))
        //    .Returns(expectedViewModel);

        //// Act
        //IActionResult result = await controller.(selectedPupils);

        //// Assert
        //ViewResult viewResult = Assert.IsType<ViewResult>(result);
        //Assert.Equal(Constants.Routes.MyPupilList.MyPupilListView, viewResult.ViewName);
        //Assert.Same(expectedViewModel, viewResult.Model);
    }

}
