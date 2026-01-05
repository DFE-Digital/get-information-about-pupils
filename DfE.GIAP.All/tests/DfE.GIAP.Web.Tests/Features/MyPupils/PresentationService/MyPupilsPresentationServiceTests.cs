using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.SharedTests.Common;
using DfE.GIAP.Web.Features.MyPupils.Controllers;
using DfE.GIAP.Web.Features.MyPupils.PresentationService;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.PresentationHandlers;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.Handlers;
using DfE.GIAP.Web.Tests.Features.MyPupils.TestDoubles;
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

    [Theory]
    [MemberData(nameof(DeletePupilsInput))]
    public async Task DeletePupilsAsync_CallsUseCase_And_ClearsPupilSelections(
        List<string>? requestSelectedPupils,
        MyPupilsPupilSelectionState selectedState,
        List<string> expectedDeletePupils)
    {
        // Arrange
        Mock<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>> useCaseMock = new();

        Mock<IClearMyPupilsPupilSelectionsHandler> clearPupilSelectionsHandlerMock = new();

        Mock<IGetMyPupilsPupilSelectionProvider> getSelectionProviderMock = new();

        getSelectionProviderMock
            .Setup(t => t.GetPupilSelections())
            .Returns(selectedState);

        const string userId = "userId";

        MyPupilsPresentationService sut = new(
            deletePupilsUseCase: useCaseMock.Object,
            getMyPupilsUseCase: new Mock<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>>().Object,
            handler: new Mock<IMyPupilsPresentationModelHandler>().Object,
            getMyPupilsStateProvider: getSelectionProviderMock.Object,
            clearMyPupilsPupilSelectionsCommandHandler: clearPupilSelectionsHandlerMock.Object,
            mapper: MapperTestDoubles.Default<MyPupilsModels, MyPupilsPresentationPupilModels>().Object
        );

        // Act
        await sut.DeletePupilsAsync(userId, requestSelectedPupils);

        getSelectionProviderMock.Verify(
            (provider) => provider.GetPupilSelections(), Times.Once);

        useCaseMock.Verify((useCase) =>
            useCase.HandleRequestAsync(
                It.Is<DeletePupilsFromMyPupilsRequest>(
                    (request) => request.UserId.Equals(userId) &&
                        request.DeletePupilUpns.SequenceEqual(expectedDeletePupils))), Times.Once());

        clearPupilSelectionsHandlerMock.Verify(
            (handler) => handler.Handle(), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("\n")]
    [InlineData(" ")]
    public async Task GetPupilsAsync_Throws_When_Request_UserId_Is_NullOrWhitespace(string? userId)
    {
        // Arrange
        MyPupilsPresentationService sut = new(
            deletePupilsUseCase: new Mock<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>().Object,
            getMyPupilsUseCase: new Mock<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>>().Object,
            handler: new Mock<IMyPupilsPresentationModelHandler>().Object,
            getMyPupilsStateProvider: new Mock<IGetMyPupilsPupilSelectionProvider>().Object,
            clearMyPupilsPupilSelectionsCommandHandler: new Mock<IClearMyPupilsPupilSelectionsHandler>().Object,
            mapper: MapperTestDoubles.Default<MyPupilsModels, MyPupilsPresentationPupilModels>().Object
        );

        // Act
        await Assert.ThrowsAnyAsync<ArgumentException>(() => sut.GetPupilsAsync(userId, It.IsAny<MyPupilsQueryRequestDto>()));
    }

    // TODO expand calls that are made to dependencies.
    [Theory]
    [MemberData(nameof(GetMyPupilsInputs))]
    public async Task GetMyPupils_Returns_Pupils(MyPupilsQueryRequestDto? request)
    {
        // Arrange
        const string userId = "userId";

        Mock<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>> useCaseMock = new();
        
        GetMyPupilsResponse useCaseResponse = new(MyPupilsModels.Create([]));

        useCaseMock
            .Setup((useCase) => useCase.HandleRequestAsync(It.IsAny<GetMyPupilsRequest>()))
            .ReturnsAsync(useCaseResponse);

        Mock<IGetMyPupilsPupilSelectionProvider> getPupilSelectionsProvider = new();

        getPupilSelectionsProvider
            .Setup(t => t.GetPupilSelections())
            .Returns(MyPupilsPupilSelectionState.CreateDefault());

        MyPupilsPresentationPupilModels outputPupils =
            MyPupilsPresentationPupilModelsTestDoubles.Generate(count: 10);

        Mock<IMapper<MyPupilsModels, MyPupilsPresentationPupilModels>> mapperMock =
            MapperTestDoubles.MockFor<MyPupilsModels, MyPupilsPresentationPupilModels>(stub: outputPupils);

        Mock<IMyPupilsPresentationModelHandler> handlerMock = new();
        handlerMock.Setup(
            (handler) => handler.Handle(
                It.IsAny<MyPupilsPresentationPupilModels>(),
                It.IsAny<MyPupilsPresentationQueryModel>(),
                It.IsAny<MyPupilsPupilSelectionState>()))
            .Returns(outputPupils);

        MyPupilsPresentationService sut = new(
            deletePupilsUseCase: new Mock<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>().Object,
            getMyPupilsUseCase: useCaseMock.Object,
            handler: handlerMock.Object,
            getMyPupilsStateProvider: getPupilSelectionsProvider.Object,
            clearMyPupilsPupilSelectionsCommandHandler: new Mock<IClearMyPupilsPupilSelectionsHandler>().Object,
            mapper: mapperMock.Object
        );

        // Act
        // TODO assert on response
        MyPupilsPresentationResponse myPupilsPresentationResponse = await sut.GetPupilsAsync(userId, request);

        getPupilSelectionsProvider.Verify(
            (provider) => provider.GetPupilSelections(),
                Times.Once);

        useCaseMock.Verify(
            (useCase) => useCase.HandleRequestAsync(It.IsAny<GetMyPupilsRequest>()),
                Times.Once);

        mapperMock.Verify(
            (mapper) => mapper.Map(It.IsAny<MyPupilsModels>()),
                Times.Once);

        handlerMock.Verify(
            (handler) => handler.Handle(
                It.IsAny<MyPupilsPresentationPupilModels>(),
                It.IsAny<MyPupilsPresentationQueryModel>(),
                It.IsAny<MyPupilsPupilSelectionState>()),
                Times.Once);
    }

    public static TheoryData<MyPupilsQueryRequestDto?> GetMyPupilsInputs
    {
        get
        {
            return new()
            {
                {
                    null!
                },
                {
                    new()
                }
            };
        }
    }

    public static TheoryData<List<string>?, MyPupilsPupilSelectionState, List<string>> DeletePupilsInput
    {
        get
        {
            return new TheoryData<List<string>?, MyPupilsPupilSelectionState, List<string>>
            {
                // null selections is defaulted
                {
                    null!,
                    MyPupilsPupilSelectionState.CreateDefault(),
                    []
                },
                // [] is handled
                {
                    [],
                    MyPupilsPupilSelectionState.CreateDefault(),
                    []
                },
                // selections are added from request to empty state
                {
                    ["1" ],
                    MyPupilsPupilSelectionState.CreateDefault(),
                    ["1"]
                },
                // selections are appended from request to state
                {
                    ["1" ],
                    MyPupilsPupilSelectionStateTestDoubles.WithPupilsSelectionState(new()
                    {
                        { ["2"] , true }
                    }),
                    ["1", "2"]
                },
                // selections are deduplicated
                {
                    ["1" ],
                    MyPupilsPupilSelectionStateTestDoubles.WithPupilsSelectionState(new()
                    {
                        {  ["1", "2"] , true }
                    }),
                    ["1", "2"]
                },
            };
        }
    }
}
