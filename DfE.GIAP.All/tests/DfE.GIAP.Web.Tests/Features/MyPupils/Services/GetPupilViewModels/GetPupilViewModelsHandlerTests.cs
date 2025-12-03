using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.SharedTests.TestDoubles;
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

namespace DfE.GIAP.Web.Tests.Features.MyPupils.Services.GetPupilViewModels;
public sealed class GetPupilViewModelsHandlerTests
{
    [Fact]
    public void Constructor_Throws_When_PaginatedHandler_Is_Null()
    {
        // Arrange
        Mock<IMapper<MyPupilsModelSelectionStateDecorator, PupilsViewModel>> mapperMock = MapperTestDoubles.Default<MyPupilsModelSelectionStateDecorator, PupilsViewModel>();
        Func<GetPupilViewModelsHandler> construct = () => new(null, mapperMock.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_Mapper_Is_Null()
    {
        // Arrange
        Mock<IGetPaginatedMyPupilsHandler> paginatedHandlerMock = IGetPaginatedMyPupilsHandlerTestDoubles.Default();
        Func<GetPupilViewModelsHandler> construct = () => new(paginatedHandlerMock.Object, null);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Request_Is_Null()
    {
        // Arrange
        Mock<IGetPaginatedMyPupilsHandler> paginatedHandlerMock = IGetPaginatedMyPupilsHandlerTestDoubles.Default();
        Mock<IMapper<MyPupilsModelSelectionStateDecorator, PupilsViewModel>> mapperMock = MapperTestDoubles.Default<MyPupilsModelSelectionStateDecorator, PupilsViewModel>();
        
        GetPupilViewModelsHandler sut = new(paginatedHandlerMock.Object, mapperMock.Object);

        Func<Task> act = async () => await sut.GetPupilsAsync(null);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_State_Is_Null()
    {
        // Arrange
        Mock<IGetPaginatedMyPupilsHandler> paginatedHandlerMock = IGetPaginatedMyPupilsHandlerTestDoubles.Default();
        Mock<IMapper<MyPupilsModelSelectionStateDecorator, PupilsViewModel>> mapperMock = MapperTestDoubles.Default<MyPupilsModelSelectionStateDecorator, PupilsViewModel>();

        GetPupilViewModelsHandler sut = new(paginatedHandlerMock.Object, mapperMock.Object);

        MyPupilsId myPupilsId = MyPupilsIdTestDoubles.Default();

        GetPupilViewModelsRequest request = new(
            UserId: myPupilsId.Value,
            State: null);

        Func<Task> act = async () => await sut.GetPupilsAsync(request);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    [Fact]
    public async Task HandleAsync_Maps_Out_Pupils()
    {
        // Arrange
        MyPupilsModel stubPaginatedMyPupilDtos = MyPupilDtosTestDoubles.Generate(count: 1);
        PaginatedMyPupilsResponse paginatedResponseStub = new(stubPaginatedMyPupilDtos);
        Mock<IGetPaginatedMyPupilsHandler> paginatedHandlerMock = IGetPaginatedMyPupilsHandlerTestDoubles.MockFor(paginatedResponseStub);

        PupilsViewModel pupils = PupilsViewModelTestDoubles.Generate(count: 10);

        Mock<IMapper<MyPupilsModelSelectionStateDecorator, PupilsViewModel>> mapperMock = MapperTestDoubles.MockFor<MyPupilsModelSelectionStateDecorator, PupilsViewModel>(pupils);

        GetPupilViewModelsHandler sut = new(paginatedHandlerMock.Object, mapperMock.Object);

        MyPupilsPupilSelectionState selectionState = new();
        MyPupilsState state = new(MyPupilsPresentationStateTestDoubles.Default(), selectionState);

        MyPupilsId myPupilsId = MyPupilsIdTestDoubles.Default();

        GetPupilViewModelsRequest request = new(UserId: myPupilsId.Value, state);

        // Act
        PupilsViewModel response = await sut.GetPupilsAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(10, response.Pupils.Count);
        Assert.Equivalent(pupils.Pupils, response.Pupils);

        paginatedHandlerMock.Verify((handler)
            => handler.HandleAsync(
                It.Is<GetPaginatedMyPupilsRequest>((request) => request.MyPupilsId.Equals(myPupilsId.Value))), Times.Once);

        mapperMock.Verify((mapper)
            => mapper.Map(
                It.Is<MyPupilsModelSelectionStateDecorator>((request) => request.Pupils.Values.SequenceEqual(stubPaginatedMyPupilDtos.Values))), Times.Once);
    }
}
