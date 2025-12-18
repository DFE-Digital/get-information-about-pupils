using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.SharedTests.Common;
using DfE.GIAP.Web.Features.MyPupils.PresentationService;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.PresentationHandlers;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.Handlers;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.PresentationService;

public sealed class MyPupilsPresentationServiceTests
{
    [Fact]
    public void Constructor_Throws_When_DeletePupilsUseCase_Is_Null()
    {
        // Arrange
        Func<MyPupilsPresentationService> construct = () => new(
            deletePupilsUseCase: null!,
            getMyPupilsUseCase: new Mock<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>>().Object,
            handler: new Mock<IMyPupilsPresentationModelHandler>().Object,
            getMyPupilsStateProvider: new Mock<IGetMyPupilsPupilSelectionProvider>().Object,
            clearMyPupilsPupilSelectionsCommandHandler: new Mock<IClearMyPupilsPupilSelectionsHandler>().Object,
            mapper: MapperTestDoubles.Default<MyPupilsModels, MyPupilsPresentationPupilModels>().Object
        );

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_GetMyPupilsUseCase_Is_Null()
    {
        // Arrange
        Func<MyPupilsPresentationService> construct = () => new(
            deletePupilsUseCase: new Mock<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>().Object,
            getMyPupilsUseCase: null!,
            handler: new Mock<IMyPupilsPresentationModelHandler>().Object,
            getMyPupilsStateProvider: new Mock<IGetMyPupilsPupilSelectionProvider>().Object,
            clearMyPupilsPupilSelectionsCommandHandler: new Mock<IClearMyPupilsPupilSelectionsHandler>().Object,
            mapper: MapperTestDoubles.Default<MyPupilsModels, MyPupilsPresentationPupilModels>().Object
        );

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_PresentationHandler_Is_Null()
    {
        // Arrange
        Func<MyPupilsPresentationService> construct = () => new(
            deletePupilsUseCase: new Mock<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>().Object,
            getMyPupilsUseCase: new Mock<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>>().Object,
            handler: null!,
            getMyPupilsStateProvider: new Mock<IGetMyPupilsPupilSelectionProvider>().Object,
            clearMyPupilsPupilSelectionsCommandHandler: new Mock<IClearMyPupilsPupilSelectionsHandler>().Object,
            mapper: MapperTestDoubles.Default<MyPupilsModels, MyPupilsPresentationPupilModels>().Object
        );

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_StateProvider_Is_Null()
    {
        // Arrange
        Func<MyPupilsPresentationService> construct = () => new(
            deletePupilsUseCase: new Mock<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>().Object,
            getMyPupilsUseCase: new Mock<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>>().Object,
            handler: new Mock<IMyPupilsPresentationModelHandler>().Object,
            getMyPupilsStateProvider: null!,
            clearMyPupilsPupilSelectionsCommandHandler: new Mock<IClearMyPupilsPupilSelectionsHandler>().Object,
            mapper: MapperTestDoubles.Default<MyPupilsModels, MyPupilsPresentationPupilModels>().Object
        );

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_ClearSelectionsHandler_Is_Null()
    {
        // Arrange
        Func<MyPupilsPresentationService> construct = () => new(
            deletePupilsUseCase: new Mock<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>().Object,
            getMyPupilsUseCase: new Mock<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>>().Object,
            handler: new Mock<IMyPupilsPresentationModelHandler>().Object,
            getMyPupilsStateProvider: new Mock<IGetMyPupilsPupilSelectionProvider>().Object,
            clearMyPupilsPupilSelectionsCommandHandler: null!,
            mapper: MapperTestDoubles.Default<MyPupilsModels, MyPupilsPresentationPupilModels>().Object
        );

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_Mapper_Is_Null()
    {
        // Arrange
        Func<MyPupilsPresentationService> construct = () => new(
            deletePupilsUseCase: new Mock<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>().Object,
            getMyPupilsUseCase: new Mock<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>>().Object,
            handler: new Mock<IMyPupilsPresentationModelHandler>().Object,
            getMyPupilsStateProvider: new Mock<IGetMyPupilsPupilSelectionProvider>().Object,
            clearMyPupilsPupilSelectionsCommandHandler: new Mock<IClearMyPupilsPupilSelectionsHandler>().Object,
            mapper: null!
        );

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }
}
