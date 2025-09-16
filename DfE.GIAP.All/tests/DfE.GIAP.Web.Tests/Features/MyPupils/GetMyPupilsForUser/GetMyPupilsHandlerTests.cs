using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.MyPupils;
using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser;
using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser.Mapper;
using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser.ViewModels;
using DfE.GIAP.Web.Features.MyPupils.Services.GetPaginatedMyPupils;
using DfE.GIAP.Web.Features.MyPupils.State;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;
using DfE.GIAP.Web.Tests.TestDoubles.MyPupils;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.GetMyPupils;
public sealed class GetMyPupilsHandlerTests
{
    [Fact]
    public void Constructor_Throws_When_PaginatedHandler_Is_Null()
    {
        // Arrange
        Mock<IMapper<MyPupilsDtoSelectionStateDecorator, PupilsViewModel>> mapperMock = MapperTestDoubles.Default<MyPupilsDtoSelectionStateDecorator, PupilsViewModel>();
        Func<GetMyPupilsForUserHandler> construct = () => new(null, mapperMock.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_Mapper_Is_Null()
    {
        // Arrange
        Mock<IGetPaginatedMyPupilsHandler> paginatedHandlerMock = IGetPaginatedMyPupilsHandlerTestDoubles.Default();
        Func<GetMyPupilsForUserHandler> construct = () => new(paginatedHandlerMock.Object, null);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Request_Is_Null()
    {
        // Arrange
        Mock<IGetPaginatedMyPupilsHandler> paginatedHandlerMock = IGetPaginatedMyPupilsHandlerTestDoubles.Default();
        Mock<IMapper<MyPupilsDtoSelectionStateDecorator, PupilsViewModel>> mapperMock = MapperTestDoubles.Default<MyPupilsDtoSelectionStateDecorator, PupilsViewModel>();
        
        GetMyPupilsForUserHandler sut = new(paginatedHandlerMock.Object, mapperMock.Object);

        Func<Task> act = async () => await sut.GetPupilsAsync(null);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_State_Is_Null()
    {
        // Arrange
        Mock<IGetPaginatedMyPupilsHandler> paginatedHandlerMock = IGetPaginatedMyPupilsHandlerTestDoubles.Default();
        Mock<IMapper<MyPupilsDtoSelectionStateDecorator, PupilsViewModel>> mapperMock = MapperTestDoubles.Default<MyPupilsDtoSelectionStateDecorator, PupilsViewModel>();

        GetMyPupilsForUserHandler sut = new(paginatedHandlerMock.Object, mapperMock.Object);

        GetMyPupilsForUserRequest request = new(
            UserId: MyPupilsIdTestDoubles.Default().Value,
            State: null);

        Func<Task> act = async () => await sut.GetPupilsAsync(request);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    [Fact]
    public async Task HandleAsync_Maps_Out_Pupils()
    {
        // Arrange
        MyPupilsId myPupilsId = MyPupilsIdTestDoubles.Default();

        MyPupilsModel stubPaginatedMyPupilDtos = MyPupilDtosTestDoubles.Generate(count: 1);
        PaginatedMyPupilsResponse paginatedResponseStub = new(stubPaginatedMyPupilDtos);
        Mock<IGetPaginatedMyPupilsHandler> paginatedHandlerMock = IGetPaginatedMyPupilsHandlerTestDoubles.MockFor(paginatedResponseStub);

        PupilsViewModel pupils = PupilsViewModelTestDoubles.Generate(count: 10);

        Mock<IMapper<MyPupilsDtoSelectionStateDecorator, PupilsViewModel>> mapperMock = MapperTestDoubles.MockFor<MyPupilsDtoSelectionStateDecorator, PupilsViewModel>(pupils);

        GetMyPupilsForUserHandler sut = new(paginatedHandlerMock.Object, mapperMock.Object);

        MyPupilsPupilSelectionState selectionState = new();
        MyPupilsState state = new(MyPupilsPresentationStateTestDoubles.Default(), selectionState);

        GetMyPupilsForUserRequest request = new(myPupilsId.Value, state);

        // Act
        PupilsViewModel response = await sut.GetPupilsAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(10, response.Pupils.Count);
        Assert.Equivalent(pupils.Pupils, response.Pupils);

        paginatedHandlerMock.Verify((handler)
            => handler.HandleAsync(
                It.Is<GetPaginatedMyPupilsRequest>((request) => request.UserId.Equals(myPupilsId.Value))), Times.Once);

        mapperMock.Verify((mapper)
            => mapper.Map(
                It.Is<MyPupilsDtoSelectionStateDecorator>((request) => request.PupilDtos.Values.SequenceEqual(stubPaginatedMyPupilDtos.Values))), Times.Once);
    }
}
