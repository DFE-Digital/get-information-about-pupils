using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.SharedTests.Common;
using DfE.GIAP.SharedTests.Features.MyPupils.Application;
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

    [Fact]
    public async Task GetPupilsAsync_Throws_When_Request_PageNumber_LessThan0()
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

        MyPupilsPresentationService sut = new(
            getMyPupilsUseCase: useCaseMock.Object,
            handler: new Mock<IMyPupilsPresentationModelHandler>().Object,
            getMyPupilsStateProvider: getPupilSelectionsProvider.Object,
            mapper: new Mock<IMapper<MyPupilsModels, MyPupilsPresentationPupilModels>>().Object,
            deletePupilsUseCase: new Mock<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>().Object,
            clearMyPupilsPupilSelectionsCommandHandler: new Mock<IClearMyPupilsPupilSelectionsHandler>().Object);

        MyPupilsQueryRequestDto request = new()
        {
            PageNumber = -1
        };

        // Act Assert
        Func<Task> act = () => sut.GetPupilsAsync(userId, request);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(act);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(100, 5)]
    [InlineData(101, 6)]
    public async Task GetMyPupils_Returns_PageSize_From_TotalPupilCount(int totalPupilCount, int expectedPages)
    {
        // Arrange
        const string userId = "userId";

        Mock<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>> useCaseMock = new();

        GetMyPupilsResponse useCaseResponseStub = new(MyPupilsModelTestDoubles.Generate(totalPupilCount));
        useCaseMock
            .Setup((useCase) => useCase.HandleRequestAsync(It.IsAny<GetMyPupilsRequest>()))
            .ReturnsAsync(useCaseResponseStub);

        MyPupilsPupilSelectionState selectionStateStub = new();
        Mock<IGetMyPupilsPupilSelectionProvider> getPupilSelectionsMock = new();
        getPupilSelectionsMock
            .Setup(t => t.GetPupilSelections())
            .Returns(selectionStateStub);

        MyPupilsPresentationPupilModels handlerOutputStub = MyPupilsPresentationPupilModels.Create([]);
        Mock<IMyPupilsPresentationModelHandler> handlerMock = new();
        handlerMock.Setup(
            (handler) => handler.Handle(
                It.IsAny<MyPupilsPresentationPupilModels>(),
                It.IsAny<MyPupilsPresentationQueryModel>(),
                It.IsAny<MyPupilsPupilSelectionState>()))
            .Returns(handlerOutputStub);

        Mock<IMapper<MyPupilsModels, MyPupilsPresentationPupilModels>> mapperMock =
            MapperTestDoubles.MockFor<MyPupilsModels, MyPupilsPresentationPupilModels>(stub: handlerOutputStub);

        MyPupilsPresentationService sut = new(
            getMyPupilsUseCase: useCaseMock.Object,
            handler: handlerMock.Object,
            getMyPupilsStateProvider: getPupilSelectionsMock.Object,
            mapper: mapperMock.Object,
            deletePupilsUseCase: new Mock<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>().Object,
            clearMyPupilsPupilSelectionsCommandHandler: new Mock<IClearMyPupilsPupilSelectionsHandler>().Object);

        // Act
        MyPupilsPresentationResponse response = await sut.GetPupilsAsync(userId, It.IsAny<MyPupilsQueryRequestDto>());

        // Assert
        Assert.NotNull(response);
        Assert.Equal(expectedPages, response.TotalPages);

        getPupilSelectionsMock.Verify(
            (provider) => provider.GetPupilSelections(),
                Times.Once);

        useCaseMock.Verify(
            (useCase) => useCase.HandleRequestAsync(It.Is<GetMyPupilsRequest>(request => request.UserId == userId)),
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


    [Theory]
    [MemberData(nameof(GetMyPupilsInputs))]
    public async Task GetMyPupils_Returns_Pupils_With_QueryParameters_Applied(
        MyPupilsQueryRequestDto? request,
        string expectedSortField,
        string expectedSortDirection)
    {
        // Arrange
        const string userId = "userId";

        Mock<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>> useCaseMock = new();

        GetMyPupilsResponse useCaseResponse = new(MyPupilsModels.Create([]));
        useCaseMock
            .Setup((useCase) => useCase.HandleRequestAsync(new GetMyPupilsRequest(userId)))
            .ReturnsAsync(useCaseResponse);

        MyPupilsPupilSelectionState pupilSelectionsStub = new();
        Mock<IGetMyPupilsPupilSelectionProvider> getPupilSelectionsMock = new();
        getPupilSelectionsMock
            .Setup(t => t.GetPupilSelections())
            .Returns(pupilSelectionsStub);

        MyPupilsPresentationPupilModels outputPupilsStub = MyPupilsPresentationPupilModelsTestDoubles.Generate(count: 10);
        Mock<IMyPupilsPresentationModelHandler> handlerMock = new();
        handlerMock.Setup(
            (handler) => handler.Handle(
                It.IsAny<MyPupilsPresentationPupilModels>(),
                It.IsAny<MyPupilsPresentationQueryModel>(),
                It.IsAny<MyPupilsPupilSelectionState>()))
            .Returns(outputPupilsStub);

        Mock<IMapper<MyPupilsModels, MyPupilsPresentationPupilModels>> mapperMock =
            MapperTestDoubles.MockFor<MyPupilsModels, MyPupilsPresentationPupilModels>(stub: outputPupilsStub);

        MyPupilsPresentationService sut = new(
            getMyPupilsUseCase: useCaseMock.Object,
            handler: handlerMock.Object,
            getMyPupilsStateProvider: getPupilSelectionsMock.Object,
            mapper: mapperMock.Object,
            deletePupilsUseCase: new Mock<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>().Object,
            clearMyPupilsPupilSelectionsCommandHandler: new Mock<IClearMyPupilsPupilSelectionsHandler>().Object
        );

        // Act
        MyPupilsPresentationResponse response = await sut.GetPupilsAsync(userId, request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(response.SortedField, expectedSortField);
        Assert.Equal(response.SortedDirection, expectedSortDirection);

        Assert.Equal(outputPupilsStub, response.MyPupils);
        Assert.Equal(1, response.PageNumber);
        Assert.Equal(1, response.TotalPages);
        Assert.False(response.IsAnyPupilsSelected);

        getPupilSelectionsMock.Verify(
            (provider) => provider.GetPupilSelections(),
                Times.Once);

        useCaseMock.Verify(
            (useCase) => useCase.HandleRequestAsync(It.Is<GetMyPupilsRequest>(request => request.UserId == userId)),
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

    [Fact]
    public async Task GetMyPupils_Returns_IfAnyPupilsSelected_From_SelectionState()
    {
        // Arrange
        const string userId = "userId";

        Mock<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>> useCaseMock = new();

        useCaseMock
            .Setup((useCase) => useCase.HandleRequestAsync(It.IsAny<GetMyPupilsRequest>()))
            .ReturnsAsync(new GetMyPupilsResponse(It.IsAny<MyPupilsModels>()));

        MyPupilsPupilSelectionState selectionStateStub =
            MyPupilsPupilSelectionStateTestDoubles.WithSelectedPupils(
                SelectionMode.Manual,
                selected: ["1"],
                deselected: []);

        Mock<IGetMyPupilsPupilSelectionProvider> getPupilSelectionsMock = new();
        getPupilSelectionsMock
            .Setup(t => t.GetPupilSelections())
            .Returns(selectionStateStub);

        MyPupilsPresentationPupilModels outputPupils = MyPupilsPresentationPupilModelsTestDoubles.Generate(count: 10);

        Mock<IMyPupilsPresentationModelHandler> handlerMock = new();
        handlerMock.Setup(
            (handler) => handler.Handle(
                MyPupilsPresentationPupilModels.Empty(),
                MyPupilsPresentationQueryModel.CreateDefault(),
                MyPupilsPupilSelectionState.CreateDefault()))
            .Returns(outputPupils);

        Mock<IMapper<MyPupilsModels, MyPupilsPresentationPupilModels>> mapperMock =
            MapperTestDoubles.MockFor<MyPupilsModels, MyPupilsPresentationPupilModels>(stub: outputPupils);

        MyPupilsPresentationService sut = new(
            getMyPupilsUseCase: useCaseMock.Object,
            handler: handlerMock.Object,
            getMyPupilsStateProvider: getPupilSelectionsMock.Object,
            mapper: mapperMock.Object,
            deletePupilsUseCase: new Mock<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>().Object,
            clearMyPupilsPupilSelectionsCommandHandler: new Mock<IClearMyPupilsPupilSelectionsHandler>().Object
        );

        // Act
        MyPupilsPresentationResponse response = await sut.GetPupilsAsync(userId, new MyPupilsQueryRequestDto());

        // Assert
        Assert.NotNull(response);
        Assert.True(response.IsAnyPupilsSelected);

        getPupilSelectionsMock.Verify(
            (provider) => provider.GetPupilSelections(),
                Times.Once);

        useCaseMock.Verify(
            (useCase) => useCase.HandleRequestAsync(It.Is<GetMyPupilsRequest>(request => request.UserId == userId)),
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

    [Fact]
    public async Task GetSelectedPupilsAsync_Returns_AllPupils_Except_Deselections_When_SelectionMode_Is_All()
    {
        // Arrange
        Mock<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>> useCaseMock = new();

        GetMyPupilsResponse useCaseResponse = new(MyPupilsModelTestDoubles.Generate(count: 10));
        useCaseMock
            .Setup((useCase) => useCase.HandleRequestAsync(It.IsAny<GetMyPupilsRequest>()))
            .ReturnsAsync(useCaseResponse);


        // Deselect a few pupils out of all pupils
        List<string> deselectedPupils = useCaseResponse.MyPupils.Identifiers.TakeLast(2).ToList();
        MyPupilsPupilSelectionState selectionStateStub =
            MyPupilsPupilSelectionStateTestDoubles.WithSelectedPupils(
                mode: SelectionMode.All,
                selected: useCaseResponse.MyPupils.Identifiers.ToList(),
                deselected: deselectedPupils);


        Mock<IGetMyPupilsPupilSelectionProvider> getPupilSelectionsMock = new();
        getPupilSelectionsMock
            .Setup(t => t.GetPupilSelections())
            .Returns(selectionStateStub);

        MyPupilsPresentationService sut = new(
            getMyPupilsUseCase: useCaseMock.Object,
            getMyPupilsStateProvider: getPupilSelectionsMock.Object,
            handler: new Mock<IMyPupilsPresentationModelHandler>().Object,
            mapper: new Mock<IMapper<MyPupilsModels, MyPupilsPresentationPupilModels>>().Object,
            deletePupilsUseCase: new Mock<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>().Object,
            clearMyPupilsPupilSelectionsCommandHandler: new Mock<IClearMyPupilsPupilSelectionsHandler>().Object
        );

        // Act
        const string userId = "userId";
        IEnumerable<string> response = await sut.GetSelectedPupilsAsync(userId);

        // Assert
        Assert.NotNull(response);
        Assert.Equivalent(response, useCaseResponse.MyPupils.Identifiers.Except(deselectedPupils));

        getPupilSelectionsMock.Verify(
            (provider) => provider.GetPupilSelections(),
                Times.Once);

        useCaseMock.Verify(
            (useCase) => useCase.HandleRequestAsync(It.Is<GetMyPupilsRequest>(request => request.UserId == userId)),
                Times.Once);
    }

    [Fact]
    public async Task GetSelectedPupilsAsync_Returns_ExplicitSelections_When_SelectionMode_Is_Not_All()
    {
        // Arrange
        List<string> explicitSelections = ["a", "b", "c"];

        // TODO back behind TestDouble/Builder
        MyPupilsPupilSelectionState selectionStateStub =
            MyPupilsPupilSelectionStateTestDoubles.WithSelectedPupils(
                mode: SelectionMode.Manual,
                selected: explicitSelections,
                deselected: []);

        Mock<IGetMyPupilsPupilSelectionProvider> getPupilSelectionsMock = new();
        getPupilSelectionsMock
            .Setup(t => t.GetPupilSelections())
            .Returns(selectionStateStub);

        MyPupilsPresentationService sut = new(
            getMyPupilsUseCase: new Mock<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>>().Object,
            getMyPupilsStateProvider: getPupilSelectionsMock.Object,
            handler: new Mock<IMyPupilsPresentationModelHandler>().Object,
            mapper: new Mock<IMapper<MyPupilsModels, MyPupilsPresentationPupilModels>>().Object,
            deletePupilsUseCase: new Mock<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>().Object,
            clearMyPupilsPupilSelectionsCommandHandler: new Mock<IClearMyPupilsPupilSelectionsHandler>().Object
        );

        // Act
        const string userId = "userId";
        IEnumerable<string> response = await sut.GetSelectedPupilsAsync(userId);

        // Assert
        Assert.NotNull(response);
        Assert.Equivalent(response, explicitSelections);

        getPupilSelectionsMock.Verify(
            (provider) => provider.GetPupilSelections(),
                Times.Once);
    }

    public static TheoryData<MyPupilsQueryRequestDto?, string, string> GetMyPupilsInputs
    {
        get
        {
            return new()
            {
                {
                    null!,
                    string.Empty,
                    string.Empty
                },
                {
                    new MyPupilsQueryRequestDto(),
                    string.Empty,
                    string.Empty
                },
                {
                    new MyPupilsQueryRequestDto()
                    {
                        SortField = null,
                        SortDirection = null,
                    },
                    string.Empty,
                    string.Empty
                },
                {
                    new MyPupilsQueryRequestDto()
                    {
                        SortField = "a",
                        SortDirection = "asc",
                    },
                    "a",
                    "asc"
                },
                {
                    new MyPupilsQueryRequestDto()
                    {
                        SortField = "Name",
                        SortDirection = "desc",
                    },
                    "Name",
                    "desc"
                },
                {
                    new MyPupilsQueryRequestDto()
                    {
                        SortField = "Name",
                        SortDirection = "Unknown_Sort_Direction",
                    },
                    "Name",
                    string.Empty
                },
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
                    MyPupilsPupilSelectionStateTestDoubles.WithSelectedPupils(SelectionMode.Manual, selected: ["2"], deselected: []),
                    ["1", "2"]
                },
                // selections are deduplicated
                {
                    ["1" ],
                    MyPupilsPupilSelectionStateTestDoubles.WithSelectedPupils(SelectionMode.Manual, selected: ["1", "2"], deselected: []),
                    ["1", "2"]
                },
            };
        }
    }
}
