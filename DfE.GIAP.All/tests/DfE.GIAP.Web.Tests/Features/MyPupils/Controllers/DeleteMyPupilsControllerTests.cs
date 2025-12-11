using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeleteAllPupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Features.MyPupils.Areas.GetMyPupils;
using DfE.GIAP.Web.Features.MyPupils.Controllers.DeleteMyPupils;
using DfE.GIAP.Web.Features.MyPupils.PresentationService;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.Query;
using DfE.GIAP.Web.Session.Abstraction.Command;
using DfE.GIAP.Web.Tests.TestDoubles.MyPupils;
using Microsoft.AspNetCore.Http;
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
        Mock<IGetMyPupilsPupilSelectionProvider> stateProviderMock = new();
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
        Mock<IGetMyPupilsPupilSelectionProvider> stateProviderMock = new();
        Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> sessionCommandHandlerMock = new();
        Mock<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>> deletePupilsUseCaseMock = new();
        Mock<IUseCaseRequestOnly<DeleteAllMyPupilsRequest>> deleteAllPupilsUseCaseMock = new();

        // Act Assert
        Func<DeleteMyPupilsController> construct = () => new(
            logger: loggerMock.Object,
            deleteAllPupilsUseCaseMock.Object,
            deletePupilsUseCaseMock.Object,
            stateProviderMock.Object,
            sessionCommandHandlerMock.Object);

        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_DeleteAllPupilsUseCase_Is_Null()
    {
        // Arrange
        Mock<ILogger<DeleteMyPupilsController>> loggerMock = new();
        Mock<IGetMyPupilsPupilSelectionProvider> stateProviderMock = new();
        Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> sessionCommandHandlerMock = new();
        Mock<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>> deletePupilsUseCaseMock = new();
        Mock<IUseCaseRequestOnly<DeleteAllMyPupilsRequest>> deleteAllPupilsUseCaseMock = new();

        // Act Assert
        Func<DeleteMyPupilsController> construct = () => new(
            logger: loggerMock.Object,
            deleteAllPupilsUseCase: null,
            deletePupilsUseCaseMock.Object,
            stateProviderMock.Object,
            sessionCommandHandlerMock.Object);

        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_DeletePupilsUseCase_Is_Null()
    {
        // Arrange
        Mock<ILogger<DeleteMyPupilsController>> loggerMock = new();
        Mock<IGetMyPupilsPupilSelectionProvider> stateProviderMock = new();
        Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> sessionCommandHandlerMock = new();
        Mock<IUseCaseRequestOnly<DeleteAllMyPupilsRequest>> deleteAllPupilsUseCaseMock = new();

        // Act Assert
        Func<DeleteMyPupilsController> construct = () => new(
            logger: loggerMock.Object,
            deleteAllPupilsUseCase: deleteAllPupilsUseCaseMock.Object,
            deleteSomePupilsUseCase: null,
            stateProviderMock.Object,
            sessionCommandHandlerMock.Object);

        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_SessionCommandHandler_Is_Null()
    {
        // Arrange
        Mock<ILogger<DeleteMyPupilsController>> loggerMock = new();
        Mock<IGetMyPupilsPupilSelectionProvider> stateProviderMock = new();
        Mock<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>> deletePupilsUseCaseMock = new();
        Mock<IUseCaseRequestOnly<DeleteAllMyPupilsRequest>> deleteAllPupilsUseCaseMock = new();

        // Act Assert
        Func<DeleteMyPupilsController> construct = () => new(
            logger: null,
            deleteAllPupilsUseCaseMock.Object,
            deletePupilsUseCaseMock.Object,
            stateProviderMock.Object,
            selectionStateSessionCommandHandler: null);

        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_GetPupilViewModelsHandler_Is_Null()
    {
        // Arrange
        Mock<ILogger<DeleteMyPupilsController>> loggerMock = new();
        Mock<IGetMyPupilsPupilSelectionProvider> stateProviderMock = new();
        Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> sessionCommandHandlerMock = new();
        Mock<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>> deletePupilsUseCaseMock = new();
        Mock<IUseCaseRequestOnly<DeleteAllMyPupilsRequest>> deleteAllPupilsUseCaseMock = new();

        // Act Assert
        Func<DeleteMyPupilsController> construct = () => new(
            logger: null,
            deleteAllPupilsUseCaseMock.Object,
            deletePupilsUseCaseMock.Object,
            stateProviderMock.Object,
            sessionCommandHandlerMock.Object);

        Assert.Throws<ArgumentNullException>(construct);
    }

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
                    MyPupilsPresentationStateTestDoubles.Default(),
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
                    MyPupilsPresentationStateTestDoubles.Default(),
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
