using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.QueryModel;
using DfE.GIAP.SharedTests.Common;
using DfE.GIAP.SharedTests.Features.MyPupils.Application;
using DfE.GIAP.Web.Features.MyPupils.Controllers;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.GetPupils;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.GetPupilSelections;
using DfE.GIAP.Web.Tests.Features.MyPupils.TestDoubles;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.Services;

public sealed class GetMyPupilsPresentationServiceTests
{
    [Fact]
    public void Constructor_Throws_When_GetMyPupilsUseCase_Is_Null()
    {
        // Arrange
        Func<GetMyPupilsPresentationService> construct = () => new(
            getMyPupilsUseCase: null!,
            getMyPupilsStateProvider: new Mock<IGetMyPupilsPupilSelectionProvider>().Object,
            mapper: MapperTestDoubles.Default<MyPupilsModels, MyPupilsPresentationPupilModels>().Object
        );

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_StateProvider_Is_Null()
    {
        // Arrange
        Func<GetMyPupilsPresentationService> construct = () => new(
            getMyPupilsUseCase: new Mock<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>>().Object,
            getMyPupilsStateProvider: null!,
            mapper: MapperTestDoubles.Default<MyPupilsModels, MyPupilsPresentationPupilModels>().Object
        );

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_Mapper_Is_Null()
    {
        // Arrange
        Func<GetMyPupilsPresentationService> construct = () => new(
            getMyPupilsUseCase: new Mock<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>>().Object,
            getMyPupilsStateProvider: new Mock<IGetMyPupilsPupilSelectionProvider>().Object,
            mapper: null!
        );

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("\n")]
    [InlineData(" ")]
    public async Task GetPupilsAsync_Throws_When_Request_UserId_Is_NullOrWhitespace(string? userId)
    {
        // Arrange
        GetMyPupilsPresentationService sut = new(
            getMyPupilsUseCase: new Mock<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>>().Object,
            getMyPupilsStateProvider: new Mock<IGetMyPupilsPupilSelectionProvider>().Object,
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

        GetMyPupilsPresentationService sut = new(
            getMyPupilsUseCase: useCaseMock.Object,
            getMyPupilsStateProvider: getPupilSelectionsProvider.Object,
            mapper: new Mock<IMapper<MyPupilsModels, MyPupilsPresentationPupilModels>>().Object);

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

        Mock<IMapper<MyPupilsModels, MyPupilsPresentationPupilModels>> mapperMock =
            MapperTestDoubles.MockFor<MyPupilsModels, MyPupilsPresentationPupilModels>(stub: handlerOutputStub);

        GetMyPupilsPresentationService sut = new(
            getMyPupilsUseCase: useCaseMock.Object,
            getMyPupilsStateProvider: getPupilSelectionsMock.Object,
            mapper: mapperMock.Object);

        // Act
        MyPupilsPresentationResponse response = await sut.GetPupilsAsync(userId, It.IsAny<MyPupilsQueryRequestDto>());

        // Assert
        Assert.NotNull(response);

        getPupilSelectionsMock.Verify(
            (provider) => provider.GetPupilSelections(),
                Times.Once);

        useCaseMock.Verify(
            (useCase) => useCase.HandleRequestAsync(It.Is<GetMyPupilsRequest>(request => request.UserId == userId)),
                Times.Once);

        mapperMock.Verify(
            (mapper) => mapper.Map(It.IsAny<MyPupilsModels>()),
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
            .Setup((useCase) =>
                useCase.HandleRequestAsync(
                    It.Is<GetMyPupilsRequest>(req => req.UserId == userId)))
            .ReturnsAsync(useCaseResponse);

        MyPupilsPupilSelectionState pupilSelectionsStub = new();
        Mock<IGetMyPupilsPupilSelectionProvider> getPupilSelectionsMock = new();
        getPupilSelectionsMock
            .Setup(t => t.GetPupilSelections())
            .Returns(pupilSelectionsStub);


        MyPupilsPresentationPupilModels outputPupilsStub = MyPupilsPresentationPupilModelsTestDoubles.Generate(count: 10);

        Mock<IMapper<MyPupilsModels, MyPupilsPresentationPupilModels>> mapperMock =
            MapperTestDoubles.MockFor<MyPupilsModels, MyPupilsPresentationPupilModels>(stub: outputPupilsStub);

        GetMyPupilsPresentationService sut = new(
            getMyPupilsUseCase: useCaseMock.Object,
            getMyPupilsStateProvider: getPupilSelectionsMock.Object,
            mapper: mapperMock.Object);

        // Act
        MyPupilsPresentationResponse response = await sut.GetPupilsAsync(userId, request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(response.SortedField, expectedSortField);
        Assert.Equal(response.SortedDirection, expectedSortDirection);

        Assert.Equal(outputPupilsStub, response.MyPupils);
        Assert.Equal(1, response.PageNumber);
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

        MyPupilsPresentationPupilModels outputPupilsStub = MyPupilsPresentationPupilModelsTestDoubles.Generate(count: 10);

        Mock<IMapper<MyPupilsModels, MyPupilsPresentationPupilModels>> mapperMock =
            MapperTestDoubles.MockFor<MyPupilsModels, MyPupilsPresentationPupilModels>(stub: outputPupilsStub);

        GetMyPupilsPresentationService sut = new(
            getMyPupilsUseCase: useCaseMock.Object,
            getMyPupilsStateProvider: getPupilSelectionsMock.Object,
            mapper: mapperMock.Object);

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


}
