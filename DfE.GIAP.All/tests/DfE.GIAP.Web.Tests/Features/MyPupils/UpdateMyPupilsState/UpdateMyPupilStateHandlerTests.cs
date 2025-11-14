using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Core.Users.Application.Models;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.MyPupils;
using DfE.GIAP.Web.Features.MyPupils;
using DfE.GIAP.Web.Features.MyPupils.GetPaginatedMyPupils;
using DfE.GIAP.Web.Features.MyPupils.State;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;
using DfE.GIAP.Web.Features.MyPupils.UpdateMyPupilsState;
using DfE.GIAP.Web.Features.MyPupils.UpdateMyPupilsState.PupilSelectionStateUpdater;
using DfE.GIAP.Web.Session.Abstraction.Command;
using DfE.GIAP.Web.Tests.TestDoubles.MyPupils;
using DfE.GIAP.Web.Tests.TestDoubles.Session;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.UpdateMyPupilsState;
public sealed class UpdateMyPupilStateHandlerTests
{

    [Fact]
    public void Constructor_Throws_When_StateUpdater_Is_Null()
    {
        // Arrange
        Mock<IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState>> mapperMock = MapperTestDoubles.Default<MyPupilsFormStateRequestDto, MyPupilsPresentationState>();
        Mock<ISessionCommandHandler<MyPupilsPresentationState>> presentationStateSessionCommandHandlerMock = ISessionCommandHandlerTestDoubles.Default<MyPupilsPresentationState>();
        Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> selectionStateSessionCommandHandlerMock = ISessionCommandHandlerTestDoubles.Default<MyPupilsPupilSelectionState>();
        Mock<IGetPaginatedMyPupilsHandler> getPaginatedMyPupilsHandlerMock = IGetPaginatedMyPupilsHandlerTestDoubles.Default();
        Func<UpdateMyPupilsStateHandler> construct = () => new(
            null,
            mapperMock.Object,
            presentationStateSessionCommandHandlerMock.Object,
            selectionStateSessionCommandHandlerMock.Object,
            getPaginatedMyPupilsHandlerMock.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_Mapper_Is_Null()
    {
        // Arrange
        Mock<IPupilSelectionStateUpdateHandler> selectionStateUpdateHandlerMock = IPupilSelectionStateUpdateHandlerTestDoubles.Default();
        Mock<ISessionCommandHandler<MyPupilsPresentationState>> presentationStateSessionCommandHandlerMock = ISessionCommandHandlerTestDoubles.Default<MyPupilsPresentationState>();
        Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> selectionStateSessionCommandHandlerMock = ISessionCommandHandlerTestDoubles.Default<MyPupilsPupilSelectionState>();
        Mock<IGetPaginatedMyPupilsHandler> getPaginatedMyPupilsHandlerMock = IGetPaginatedMyPupilsHandlerTestDoubles.Default();
        Func<UpdateMyPupilsStateHandler> construct = () => new(
            selectionStateUpdateHandlerMock.Object,
            null,
            presentationStateSessionCommandHandlerMock.Object,
            selectionStateSessionCommandHandlerMock.Object,
            getPaginatedMyPupilsHandlerMock.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_PresentationStateSessionCommandHandler_Is_Null()
    {
        // Arrange
        Mock<IPupilSelectionStateUpdateHandler> selectionStateUpdateHandlerMock = IPupilSelectionStateUpdateHandlerTestDoubles.Default();
        Mock<IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState>> mapperMock = MapperTestDoubles.Default<MyPupilsFormStateRequestDto, MyPupilsPresentationState>();
        Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> selectionStateSessionCommandHandlerMock = ISessionCommandHandlerTestDoubles.Default<MyPupilsPupilSelectionState>();
        Mock<IGetPaginatedMyPupilsHandler> getPaginatedMyPupilsHandlerMock = IGetPaginatedMyPupilsHandlerTestDoubles.Default();
        Func<UpdateMyPupilsStateHandler> construct = () => new(
            selectionStateUpdateHandlerMock.Object,
            mapperMock.Object,
            null,
            selectionStateSessionCommandHandlerMock.Object,
            getPaginatedMyPupilsHandlerMock.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_SelectionStateSessionCommandHandler_Is_Null()
    {
        // Arrange
        Mock<IPupilSelectionStateUpdateHandler> selectionStateUpdateHandlerMock = IPupilSelectionStateUpdateHandlerTestDoubles.Default();
        Mock<IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState>> mapperMock = MapperTestDoubles.Default<MyPupilsFormStateRequestDto, MyPupilsPresentationState>();
        Mock<ISessionCommandHandler<MyPupilsPresentationState>> presentationStateSessionCommandHandlerMock = ISessionCommandHandlerTestDoubles.Default<MyPupilsPresentationState>();
        Mock<IGetPaginatedMyPupilsHandler> getPaginatedMyPupilsHandlerMock = IGetPaginatedMyPupilsHandlerTestDoubles.Default();
        Func<UpdateMyPupilsStateHandler> construct = () => new(
            selectionStateUpdateHandlerMock.Object,
            mapperMock.Object,
            presentationStateSessionCommandHandlerMock.Object,
            null,
            getPaginatedMyPupilsHandlerMock.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_GetPaginatedPupilsHandler_Is_Null()
    {
        // Arrange
        Mock<IPupilSelectionStateUpdateHandler> selectionStateUpdateHandlerMock = IPupilSelectionStateUpdateHandlerTestDoubles.Default();
        Mock<IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState>> mapperMock = MapperTestDoubles.Default<MyPupilsFormStateRequestDto, MyPupilsPresentationState>();
        Mock<ISessionCommandHandler<MyPupilsPresentationState>> presentationStateSessionCommandHandlerMock = ISessionCommandHandlerTestDoubles.Default<MyPupilsPresentationState>();
        Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> selectionStateSessionCommandHandlerMock = ISessionCommandHandlerTestDoubles.Default<MyPupilsPupilSelectionState>();

        Func<UpdateMyPupilsStateHandler> construct = () => new(
            selectionStateUpdateHandlerMock.Object,
            mapperMock.Object,
            presentationStateSessionCommandHandlerMock.Object,
            selectionStateSessionCommandHandlerMock.Object,
            null);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Request_Is_Null()
    {
        // Arrange
        Mock<IPupilSelectionStateUpdateHandler> selectionStateUpdateHandlerMock = IPupilSelectionStateUpdateHandlerTestDoubles.Default();
        Mock<IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState>> mapperMock = MapperTestDoubles.Default<MyPupilsFormStateRequestDto, MyPupilsPresentationState>();
        Mock<ISessionCommandHandler<MyPupilsPresentationState>> presentationStateSessionCommandHandlerMock = ISessionCommandHandlerTestDoubles.Default<MyPupilsPresentationState>();
        Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> selectionStateSessionCommandHandlerMock = ISessionCommandHandlerTestDoubles.Default<MyPupilsPupilSelectionState>();
        Mock<IGetPaginatedMyPupilsHandler> getPaginatedMyPupilsHandlerMock = IGetPaginatedMyPupilsHandlerTestDoubles.Default();

        UpdateMyPupilsStateHandler sut = new(
            selectionStateUpdateHandlerMock.Object,
            mapperMock.Object,
            presentationStateSessionCommandHandlerMock.Object,
            selectionStateSessionCommandHandlerMock.Object,
            getPaginatedMyPupilsHandlerMock.Object);

        Func<Task> act = async () => await sut.HandleAsync(null!);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Request_State_Is_Null()
    {
        // Arrange
        Mock<IPupilSelectionStateUpdateHandler> selectionStateUpdateHandlerMock = IPupilSelectionStateUpdateHandlerTestDoubles.Default();
        Mock<IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState>> mapperMock = MapperTestDoubles.Default<MyPupilsFormStateRequestDto, MyPupilsPresentationState>();
        Mock<ISessionCommandHandler<MyPupilsPresentationState>> presentationStateSessionCommandHandlerMock = ISessionCommandHandlerTestDoubles.Default<MyPupilsPresentationState>();
        Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> selectionStateSessionCommandHandlerMock = ISessionCommandHandlerTestDoubles.Default<MyPupilsPupilSelectionState>();
        Mock<IGetPaginatedMyPupilsHandler> getPaginatedMyPupilsHandlerMock = IGetPaginatedMyPupilsHandlerTestDoubles.Default();

        UpdateMyPupilsStateHandler sut = new(
            selectionStateUpdateHandlerMock.Object,
            mapperMock.Object,
            presentationStateSessionCommandHandlerMock.Object,
            selectionStateSessionCommandHandlerMock.Object,
            getPaginatedMyPupilsHandlerMock.Object);

        UpdateMyPupilsStateRequest request = new( // TODO builder / factory around request creation?
            UserIdTestDoubles.Default(),
            null,
            new MyPupilsFormStateRequestDto());

        Func<Task> act = async () => await sut.HandleAsync(null!);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Request_FormUpdateState_Is_Null()
    {
        // Arrange
        Mock<IPupilSelectionStateUpdateHandler> selectionStateUpdateHandlerMock = IPupilSelectionStateUpdateHandlerTestDoubles.Default();
        Mock<IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState>> mapperMock = MapperTestDoubles.Default<MyPupilsFormStateRequestDto, MyPupilsPresentationState>();
        Mock<ISessionCommandHandler<MyPupilsPresentationState>> presentationStateSessionCommandHandlerMock = ISessionCommandHandlerTestDoubles.Default<MyPupilsPresentationState>();
        Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> selectionStateSessionCommandHandlerMock = ISessionCommandHandlerTestDoubles.Default<MyPupilsPupilSelectionState>();
        Mock<IGetPaginatedMyPupilsHandler> getPaginatedMyPupilsHandlerMock = IGetPaginatedMyPupilsHandlerTestDoubles.Default();

        UpdateMyPupilsStateHandler sut = new(
            selectionStateUpdateHandlerMock.Object,
            mapperMock.Object,
            presentationStateSessionCommandHandlerMock.Object,
            selectionStateSessionCommandHandlerMock.Object,
            getPaginatedMyPupilsHandlerMock.Object);

        MyPupilsPupilSelectionState selectionState = MyPupilsPupilSelectionStateTestDoubles.Default();
        MyPupilsPresentationState presentationState = MyPupilsPresentationStateTestDoubles.Default();
        MyPupilsState state = new(presentationState, selectionState);

        UpdateMyPupilsStateRequest request = new(
            UserIdTestDoubles.Default(),
            state,
            null);

        Func<Task> act = async () => await sut.HandleAsync(null!);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    [Fact]
    public async Task HandleAsync_UpdateSelectionState_And_PresentationState()
    {
        // Arrange
        Mock<IPupilSelectionStateUpdateHandler> selectionStateUpdateHandlerMock = IPupilSelectionStateUpdateHandlerTestDoubles.Default();

        Mock<IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState>> mapperMock =
            MapperTestDoubles.Default<MyPupilsFormStateRequestDto, MyPupilsPresentationState>();

        Mock<ISessionCommandHandler<MyPupilsPresentationState>> presentationStateSessionCommandHandlerMock =
            ISessionCommandHandlerTestDoubles.Default<MyPupilsPresentationState>();

        Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> selectionStateSessionCommandHandlerMock =
            ISessionCommandHandlerTestDoubles.Default<MyPupilsPupilSelectionState>();

        MyPupilDtos myPupilDtos = MyPupilDtosTestDoubles.Generate(count: 5);

        Mock<IGetPaginatedMyPupilsHandler> getPaginatedMyPupilsHandlerMock =
            IGetPaginatedMyPupilsHandlerTestDoubles.MockFor(
                new PaginatedMyPupilsResponse(Pupils: myPupilDtos));

        UpdateMyPupilsStateHandler sut = new(
            selectionStateUpdateHandlerMock.Object,
            mapperMock.Object,
            presentationStateSessionCommandHandlerMock.Object,
            selectionStateSessionCommandHandlerMock.Object,
            getPaginatedMyPupilsHandlerMock.Object);

        MyPupilsPupilSelectionState selectionState = MyPupilsPupilSelectionStateTestDoubles.Default();
        MyPupilsPresentationState presentationState = MyPupilsPresentationStateTestDoubles.Default();

        MyPupilsFormStateRequestDto updateStateInput = new()
        {
            SelectAll = true,
            SelectedPupils = ["A", "B", "C"]
        };

        UserId userId = UserIdTestDoubles.Default();

        UpdateMyPupilsStateRequest request = new(
            userId,
            MyPupilsStateTestDoubles.Create(presentationState, selectionState),
            updateStateInput);

        MyPupilsPresentationState mappedPresentationState = MyPupilsPresentationStateTestDoubles.Default();
        mapperMock.Setup(m => m.Map(updateStateInput)).Returns(mappedPresentationState);

        // Act
        await sut.HandleAsync(request);

        // Assert
        getPaginatedMyPupilsHandlerMock.Verify(getPaginatedMyPupilsHANDLER
            => getPaginatedMyPupilsHANDLER.HandleAsync(
                It.Is<GetPaginatedMyPupilsRequest>((request) => request.UserId.Equals(userId))), Times.Once);

        selectionStateUpdateHandlerMock.Verify((selectionStateUpdater)
            => selectionStateUpdater.Handle(selectionState, myPupilDtos.Identifiers, updateStateInput), Times.Once);

        selectionStateSessionCommandHandlerMock.Verify((selectionStateSessionCommandHandler)
            => selectionStateSessionCommandHandler.StoreInSession(selectionState), Times.Once);

        mapperMock.Verify((mapper)
            => mapper.Map(updateStateInput), Times.Once);

        presentationStateSessionCommandHandlerMock.Verify(presentationStateSessionCommandHandler
            => presentationStateSessionCommandHandler.StoreInSession(mappedPresentationState), Times.Once);
    }
}
