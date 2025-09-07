using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.SharedTests.TestDoubles;
using DfE.GIAP.Web.Features.MyPupils;
using DfE.GIAP.Web.Features.MyPupils.GetPaginatedMyPupils;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;
using DfE.GIAP.Web.Features.MyPupils.UpdateMyPupilsState;
using DfE.GIAP.Web.Session.Abstraction.Command;
using DfE.GIAP.Web.Tests.TestDoubles.MyPupils;
using DfE.GIAP.Web.Tests.TestDoubles.Session;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.UpdateMyPupilsState;
public sealed class UpdateMyPupilStateHandlerTests
{

    [Fact]
    public void Constructor_Throws_When_Mapper_Is_Null()
    {
        // Arrange
        Mock<ISessionCommandHandler<MyPupilsPresentationState>> presentationStateSessionCommandHandlerMock = ISessionCommandHandlerTestDoubles.Default<MyPupilsPresentationState>();
        Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> selectionStateSessionCommandHandlerMock = ISessionCommandHandlerTestDoubles.Default<MyPupilsPupilSelectionState>();
        Mock<IGetPaginatedMyPupilsHandler> getPaginatedMyPupilsHandlerMock = IGetPaginatedMyPupilsHandlerTestDoubles.Default();
        Func<UpdateMyPupilsStateHandler> construct = () => new(
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
        Mock<IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState>> mapperMock = MapperTestDoubles.Default<MyPupilsFormStateRequestDto, MyPupilsPresentationState>();
        Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> selectionStateSessionCommandHandlerMock = ISessionCommandHandlerTestDoubles.Default<MyPupilsPupilSelectionState>();
        Mock<IGetPaginatedMyPupilsHandler> getPaginatedMyPupilsHandlerMock = IGetPaginatedMyPupilsHandlerTestDoubles.Default();
        Func<UpdateMyPupilsStateHandler> construct = () => new(
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
        Mock<IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState>> mapperMock = MapperTestDoubles.Default<MyPupilsFormStateRequestDto, MyPupilsPresentationState>();
        Mock<ISessionCommandHandler<MyPupilsPresentationState>> presentationStateSessionCommandHandlerMock = ISessionCommandHandlerTestDoubles.Default<MyPupilsPresentationState>();
        Mock<IGetPaginatedMyPupilsHandler> getPaginatedMyPupilsHandlerMock = IGetPaginatedMyPupilsHandlerTestDoubles.Default();
        Func<UpdateMyPupilsStateHandler> construct = () => new(
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
        Mock<IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState>> mapperMock = MapperTestDoubles.Default<MyPupilsFormStateRequestDto, MyPupilsPresentationState>();
        Mock<ISessionCommandHandler<MyPupilsPresentationState>> presentationStateSessionCommandHandlerMock = ISessionCommandHandlerTestDoubles.Default<MyPupilsPresentationState>();
        Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> selectionStateSessionCommandHandlerMock = ISessionCommandHandlerTestDoubles.Default<MyPupilsPupilSelectionState>();

        Func<UpdateMyPupilsStateHandler> construct = () => new(
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
        Mock<IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState>> mapperMock = MapperTestDoubles.Default<MyPupilsFormStateRequestDto, MyPupilsPresentationState>();
        Mock<ISessionCommandHandler<MyPupilsPresentationState>> presentationStateSessionCommandHandlerMock = ISessionCommandHandlerTestDoubles.Default<MyPupilsPresentationState>();
        Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> selectionStateSessionCommandHandlerMock = ISessionCommandHandlerTestDoubles.Default<MyPupilsPupilSelectionState>();
        Mock<IGetPaginatedMyPupilsHandler> getPaginatedMyPupilsHandlerMock = IGetPaginatedMyPupilsHandlerTestDoubles.Default();

        UpdateMyPupilsStateHandler sut = new(
            mapperMock.Object,
            presentationStateSessionCommandHandlerMock.Object,
            selectionStateSessionCommandHandlerMock.Object,
            getPaginatedMyPupilsHandlerMock.Object);

        Func<Task> act = async () => await sut.HandleAsync(null!);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }
}
