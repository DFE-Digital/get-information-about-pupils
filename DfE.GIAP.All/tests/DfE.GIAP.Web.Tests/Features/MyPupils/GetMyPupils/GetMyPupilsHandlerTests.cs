using Bogus;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.SharedTests.TestDoubles;
using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.MyPupils;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.Mapper;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.ViewModel;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils;
using DfE.GIAP.Web.Features.MyPupils.State;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;
using DfE.GIAP.Web.Tests.TestDoubles.MyPupils;
using Moq;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.GetMyPupils;
public sealed class GetMyPupilsHandlerTests
{
    [Fact]
    public void Constructor_Throws_When_PaginatedHandler_Is_Null()
    {
        // Arrange
        Mock<IMapper<MyPupilsDtoSelectionStateDecorator, PupilsViewModel>> mapperMock = MapperTestDoubles.Default<MyPupilsDtoSelectionStateDecorator, PupilsViewModel>();
        Func<GetMyPupilsHandler> construct = () => new(null, mapperMock.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_Mapper_Is_Null()
    {
        // Arrange
        Mock<IGetPaginatedMyPupilsHandler> paginatedHandlerMock = IGetPaginatedMyPupilsHandlerTestDoubles.Default();
        Func<GetMyPupilsHandler> construct = () => new(paginatedHandlerMock.Object, null);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Request_Is_Null()
    {
        // Arrange
        Mock<IGetPaginatedMyPupilsHandler> paginatedHandlerMock = IGetPaginatedMyPupilsHandlerTestDoubles.Default();
        Mock<IMapper<MyPupilsDtoSelectionStateDecorator, PupilsViewModel>> mapperMock = MapperTestDoubles.Default<MyPupilsDtoSelectionStateDecorator, PupilsViewModel>();

        GetMyPupilsHandler sut = new(paginatedHandlerMock.Object, mapperMock.Object);

        Func<Task> act = async () => await sut.HandleAsync(null);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_State_Is_Null()
    {
        // Arrange
        Mock<IGetPaginatedMyPupilsHandler> paginatedHandlerMock = IGetPaginatedMyPupilsHandlerTestDoubles.Default();
        Mock<IMapper<MyPupilsDtoSelectionStateDecorator, PupilsViewModel>> mapperMock = MapperTestDoubles.Default<MyPupilsDtoSelectionStateDecorator, PupilsViewModel>();

        GetMyPupilsHandler sut = new(paginatedHandlerMock.Object, mapperMock.Object);

        GetMyPupilsRequest request = new(
            UserId: It.IsAny<string>(),
            State: null);

        Func<Task> act = async () => await sut.HandleAsync(request);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_State_PresentationState_Is_Null()
    {
        // Arrange
        Mock<IGetPaginatedMyPupilsHandler> paginatedHandlerMock = IGetPaginatedMyPupilsHandlerTestDoubles.Default();
        Mock<IMapper<MyPupilsDtoSelectionStateDecorator, PupilsViewModel>> mapperMock = MapperTestDoubles.Default<MyPupilsDtoSelectionStateDecorator, PupilsViewModel>();

        GetMyPupilsHandler sut = new(paginatedHandlerMock.Object, mapperMock.Object);

        MyPupilsState state = new(
            PresentationState: null,
            It.IsAny<MyPupilsPupilSelectionState>());

        GetMyPupilsRequest request = new(
            UserId: It.IsAny<string>(),
            state);

        Func<Task> act = async () => await sut.HandleAsync(request);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    [Fact]
    public async Task HandleAsync_Maps_Out_Pupils()
    {
        // Arrange
        UserId userId = UserIdTestDoubles.Default();

        MyPupilDtos myPupilDtos = MyPupilDtosTestDoubles.Generate(count: 1);
        PaginatedMyPupilsResponse paginatedResponseStub = new(myPupilDtos);
        Mock<IGetPaginatedMyPupilsHandler> paginatedHandlerMock = IGetPaginatedMyPupilsHandlerTestDoubles.MockFor(paginatedResponseStub);

        PupilsViewModel pupils = PupilsViewModelTestDoubles.Generate(count: 10);

        Mock<IMapper<MyPupilsDtoSelectionStateDecorator, PupilsViewModel>> mapperMock = MapperTestDoubles.MockFor<MyPupilsDtoSelectionStateDecorator, PupilsViewModel>(pupils);

        GetMyPupilsHandler sut = new(paginatedHandlerMock.Object, mapperMock.Object);

        MyPupilsPupilSelectionState selectionState = new();
        MyPupilsState state = new(MyPupilsPresentationStateTestDoubles.Default(), selectionState);

        GetMyPupilsRequest request = new(UserId: userId.Value, state);

        // Act
        PupilsViewModel response = await sut.HandleAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(10, response.Pupils.Count);
        Assert.Equivalent(pupils.Pupils, response.Pupils);

        paginatedHandlerMock.Verify((handler)
            => handler.HandleAsync(
                It.Is<GetPaginatedMyPupilsRequest>((request)
                    => request.UserId == userId.Value)), Times.Once);

        mapperMock.Verify((mapper)
            => mapper.Map(
                It.Is<MyPupilsDtoSelectionStateDecorator>((request)
                    => request.PupilDtos.Values.SequenceEqual(myPupilDtos.Values))), Times.Once);
    }
}
