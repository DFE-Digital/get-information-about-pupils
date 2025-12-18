using DfE.GIAP.SharedTests.Common;
using DfE.GIAP.Web.Features.MyPupils.Controllers.GetMyPupils;
using DfE.GIAP.Web.Features.MyPupils.PresentationService;
using DfE.GIAP.Web.Tests.Features.MyPupils.TestDoubles;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.Controllers;
public sealed class GetMyPupilsControllerTests
{
    [Fact]
    public void Constructor_Throws_When_Logger_Is_Null()
    {
        // Arrange
        Func<GetMyPupilsController> construct = () => new(
            logger: null,
            IMyPupilsPresentationServiceTestDoubles.DefaultMock().Object,
            MapperTestDoubles.Default<MyPupilsPresentationResponse, MyPupilsViewModel>().Object
        );

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

  /*  [Fact]
    public void Constructor_Throws_When_ViewModelFactory_Is_Null()
    {
        // Arrange
        InMemoryLogger<GetMyPupilsController> inMemoryLogger = LoggerTestDoubles.MockLogger<GetMyPupilsController>();
        
        // Act Assert
        Func<GetMyPupilsController> construct = () => new(
            inMemoryLogger,
            );

        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_StateProvider_Is_Null()
    {
        // Arrange
        InMemoryLogger<GetMyPupilsController> inMemoryLogger = LoggerTestDoubles.MockLogger<GetMyPupilsController>();
        
        // Act Assert
        Func<GetMyPupilsController> construct = () => new(
            inMemoryLogger,
            );

        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_Handler_Is_Null()
    {
        // Arrange
        InMemoryLogger<GetMyPupilsController> inMemoryLogger = LoggerTestDoubles.MockLogger<GetMyPupilsController>();
        
        // Act Assert
        Func<GetMyPupilsController> construct = () => new(
            inMemoryLogger,
            );

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

        Mock<IGetMyPupilsPupilSelectionProvider> stateProviderMock = new();
        stateProviderMock
            .Setup(stateProvider => stateProvider.GetPupilSelections())
            .Returns(state)
            .Verifiable();

        
        MyPupilsPresentationPupilModels pupilsViewModel = MyPupilsPresentationModelTestDoubles.Generate(count: 10);
        MyPupilsResponse response = new(pupilsViewModel);

        Mock<IGetMyPupilsHandler> handlerMock = new();
        handlerMock
            .Setup(handler => handler.GetPupilsAsync(It.IsAny<MyPupilsRequest>()))
            .ReturnsAsync(response);

        Mock<IMyPupilsViewModelFactory> viewModelFactoryMock = new();
        viewModelFactoryMock.Setup(
            (factory)
                => factory.CreateViewModel(
                        It.IsAny<MyPupilsState>(),
                        It.IsAny<MyPupilsPresentationPupilModels>(),
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

        Assert.Equal(viewModel.CurrentPageOfPupils, pupilsViewModel);
        Assert.True(viewModel.HasPupils);

        Assert.Equal(1, viewModel.PageNumber);
        Assert.Equal(string.Empty, viewModel.SortDirection);
        Assert.Equal(string.Empty, viewModel.SortField);
        Assert.False(viewModel.IsAllPupilsSelected);;

        Assert.False(viewModel.IsAnyPupilsSelected);
        Assert.False(viewModel.IsAnyPupilsSelected);
        Assert.False(viewModel.IsDeleteSuccessful);
    }*/
}
