using DfE.GIAP.Core.SharedTests.TestDoubles;
using DfE.GIAP.Web.Features.MyPupils.Routes;
using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser;
using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser.ViewModels;
using DfE.GIAP.Web.Features.MyPupils.State;
using DfE.GIAP.Web.Features.MyPupils.ViewModel;
using DfE.GIAP.Web.Features.MyPupils.ViewModels.Factory;
using DfE.GIAP.Web.Tests.TestDoubles.MyPupils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.Controllers;
public sealed class GetMyPupilsControllerTests
{
    [Fact]
    public void Constructor_Throws_When_Logger_Is_Null()
    {
        // Arrange
        Mock<IGetMyPupilsStateProvider> stateProviderMock = new();
        Mock<IGetPupilViewModelsHandler> handlerMock = new();
        Mock<IMyPupilsViewModelFactory> viewModelFactoryMock = new();
        // Act Assert
        Func<GetMyPupilsController> construct = () => new(
            logger: null,
            viewModelFactoryMock.Object,
            stateProviderMock.Object,
            handlerMock.Object);

        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_ViewModelFactory_Is_Null()
    {
        // Arrange
        InMemoryLogger<GetMyPupilsController> inMemoryLogger = LoggerTestDoubles.MockLogger<GetMyPupilsController>();
        Mock<IGetMyPupilsStateProvider> stateProviderMock = new();
        Mock<IGetPupilViewModelsHandler> handlerMock = new();

        // Act Assert
        Func<GetMyPupilsController> construct = () => new(
            inMemoryLogger,
            viewModelFactory: null,
            stateProviderMock.Object,
            handlerMock.Object);

        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_StateProvider_Is_Null()
    {
        // Arrange
        InMemoryLogger<GetMyPupilsController> inMemoryLogger = LoggerTestDoubles.MockLogger<GetMyPupilsController>();
        Mock<IMyPupilsViewModelFactory> viewModelFactoryMock = new();
        Mock<IGetPupilViewModelsHandler> handlerMock = new();

        // Act Assert
        Func<GetMyPupilsController> construct = () => new(
            inMemoryLogger,
            viewModelFactory: viewModelFactoryMock.Object,
            stateProvider: null,
            handlerMock.Object);

        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_Handler_Is_Null()
    {
        // Arrange
        InMemoryLogger<GetMyPupilsController> inMemoryLogger = LoggerTestDoubles.MockLogger<GetMyPupilsController>();
        Mock<IGetMyPupilsStateProvider> stateProviderMock = new();
        Mock<IMyPupilsViewModelFactory> viewModelFactoryMock = new();

        // Act Assert
        Func<GetMyPupilsController> construct = () => new(
            inMemoryLogger,
            viewModelFactory: viewModelFactoryMock.Object,
            stateProvider: stateProviderMock.Object,
            getPupilViewModelsHandler: null);

        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task Index_Returns_PupilViewModels()
    {
        // Arrange
        const string MyPupilsView = "~/Views/MyPupilList/Index.cshtml";

        InMemoryLogger<GetMyPupilsController> inMemoryLogger = LoggerTestDoubles.MockLogger<GetMyPupilsController>();
        
        MyPupilsState state = MyPupilsStateTestDoubles.Create(
            MyPupilsPresentationStateTestDoubles.Default(),
            MyPupilsPupilSelectionStateTestDoubles.Default());

        Mock<IGetMyPupilsStateProvider> stateProviderMock = new();
        stateProviderMock
            .Setup(stateProvider => stateProvider.GetState())
            .Returns(state)
            .Verifiable();

        PupilsViewModel pupilsViewModel = PupilsViewModelTestDoubles.Generate(count: 10);

        Mock<IGetPupilViewModelsHandler> handlerMock = new();
        handlerMock
            .Setup(handler => handler.GetPupilsAsync(It.IsAny<GetPupilViewModelsRequest>()))
            .ReturnsAsync(pupilsViewModel);

        Mock<IMyPupilsViewModelFactory> viewModelFactoryMock = new();
        viewModelFactoryMock.Setup(
            (factory)
                => factory.CreateViewModel(
                        It.IsAny<MyPupilsState>(),
                        It.IsAny<PupilsViewModel>(),
                        It.IsAny<MyPupilsViewModelContext>()))
                .Returns(new MyPupilsViewModel(pupilsViewModel))
                .Verifiable();

        // Act
        GetMyPupilsController sut = new(
            inMemoryLogger,
            viewModelFactoryMock.Object,
            stateProviderMock.Object,
            handlerMock.Object);

        HttpContext context = sut.StubHttpContext();
        sut.StubTempData([] ,context);

        // Assert
        IActionResult actionResult = await sut.Index();
        ViewResult viewResult = Assert.IsType<ViewResult>(actionResult);
        Assert.NotNull(viewResult);
        Assert.Equal(MyPupilsView, viewResult.ViewName);

        MyPupilsViewModel viewModel = Assert.IsType<MyPupilsViewModel>(viewResult.Model);
        Assert.NotNull(viewModel);
        Assert.NotNull(viewModel.Error);
        Assert.False(viewModel.Error.HasErrorMessage);

        Assert.Equal(viewModel.Pupils, pupilsViewModel);
        Assert.True(viewModel.HasPupils);

        Assert.Equal(1, viewModel.PageNumber);
        Assert.Equal(string.Empty, viewModel.SortDirection);
        Assert.Equal(string.Empty, viewModel.SortField);
        Assert.False(viewModel.SelectAll);;

        Assert.False(viewModel.IsAnyPupilsSelected);
        Assert.False(viewModel.IsAnyPupilsSelected);
        Assert.False(viewModel.IsDeleteSuccessful);
    }
}
