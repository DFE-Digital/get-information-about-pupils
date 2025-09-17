using DfE.GIAP.Core.SharedTests.TestDoubles;
using DfE.GIAP.Web.Features.MyPupils.Routes;
using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser;
using DfE.GIAP.Web.Features.MyPupils.State;
using DfE.GIAP.Web.Features.MyPupils.ViewModel;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.Controllers;
public sealed class GetMyPupilsControllerTests
{
    [Fact]
    public void T1()
    {
        // Arrange

        // Act

        GetMyPupilsController sut = new(null, null, null, null);
        sut.StubHttpContext();

        // Assert
        Assert.True(true);
    }

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

}
