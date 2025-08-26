using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Controllers.MyPupilList;
using DfE.GIAP.Web.Controllers.MyPupilList.Handlers.UpdatePresentationState;
using DfE.GIAP.Web.Controllers.MyPupilList.PresentationState;
using DfE.GIAP.Web.Controllers.MyPupilList.PresentationState.Provider;
using DfE.GIAP.Web.Controllers.MyPupilList.PupilSelectionState;
using DfE.GIAP.Web.Controllers.MyPupilList.PupilSelectionState.Provider;
using DfE.GIAP.Web.Tests.Controllers.MyPupilList.TestDoubles;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.MyPupilList.Handlers;
public sealed class UpdateMyPupilsStateCommandHandlerTests
{

    [Fact]
    public void Constructor_Throws_When_Mapper_Is_Null()
    {
        Mock<IMyPupilsPresentationStateProvider> myPupilsPresentationStateProvider = MyPupilsPresentationStateProviderTestDoubles.Default();
        Mock<IPupilSelectionStateProvider> pupilSelectionStateProvider = PupilSelectionStateProviderTestDoubles.Default();

        Func<UpdateMyPupilsStateCommandHandler> construct = () => new(
            null,
            myPupilsPresentationStateProvider.Object,
            pupilSelectionStateProvider.Object);

        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_PresentationStateProvider_Is_Null()
    {
        Mock<IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState>> mapper = MapperTestDoubles.Default<MyPupilsFormStateRequestDto, MyPupilsPresentationState>();
        Mock<IPupilSelectionStateProvider> pupilSelectionStateProvider = PupilSelectionStateProviderTestDoubles.Default();

        Func<UpdateMyPupilsStateCommandHandler> construct = () => new(
            mapper.Object,
            null,
            pupilSelectionStateProvider.Object);

        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_PupilSelectionStateProvider_Is_Null()
    {
        Mock<IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState>> mapper = MapperTestDoubles.Default<MyPupilsFormStateRequestDto, MyPupilsPresentationState>();
        Mock<IMyPupilsPresentationStateProvider> myPupilsPresentationStateProvider = MyPupilsPresentationStateProviderTestDoubles.Default();

        Func<UpdateMyPupilsStateCommandHandler> construct = () => new(
            mapper.Object,
            myPupilsPresentationStateProvider.Object,
            null);

        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Handle_Throws_When_FormState_Is_Null()
    {
        Mock<IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState>> mapper = MapperTestDoubles.Default<MyPupilsFormStateRequestDto, MyPupilsPresentationState>();
        Mock<IMyPupilsPresentationStateProvider> myPupilsPresentationStateProvider = MyPupilsPresentationStateProviderTestDoubles.Default();
        Mock<IPupilSelectionStateProvider> pupilSelectionStateProvider = PupilSelectionStateProviderTestDoubles.Default();


        UpdateMyPupilsStateCommandHandler sut = new(
            mapper.Object,
            myPupilsPresentationStateProvider.Object,
            pupilSelectionStateProvider.Object);

        Action act = () => sut.Handle(request: null);

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Handle_When_SelectingAllPupils_SelectsAllPupils_And_SavesSelectionState()
    {
        // Arrange
        Mock<IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState>> mapper = MapperTestDoubles.Default<MyPupilsFormStateRequestDto, MyPupilsPresentationState>();
        Mock<IMyPupilsPresentationStateProvider> myPupilsPresentationStateProvider = MyPupilsPresentationStateProviderTestDoubles.Default();
        Mock<IPupilSelectionStateProvider> pupilSelectionStateProvider = PupilSelectionStateProviderTestDoubles.Default();
        Mock<IMyPupilsPupilSelectionState> initialState = PupilsSelectionStateTestDoubles.Default();
        MyPupilsFormStateRequestDto request = MyPupilsFormStateRequestDtoTestDoubles.CreateWithSelectionState(selectAll: true);
        Assert.NotEmpty(request.ParseCurrentPageOfPupils());

        mapper.Setup((t) => t.Map(request)).Returns(It.IsAny<MyPupilsPresentationState>());

        pupilSelectionStateProvider.Setup(t => t.GetState()).Returns(initialState.Object);

        UpdateMyPupilsStateCommandHandler sut = new(
            mapper.Object,
            myPupilsPresentationStateProvider.Object,
            pupilSelectionStateProvider.Object);

        // Act
        sut.Handle(request);

        // Assert
        // PresentationState
        mapper.Verify(t => t.Map(request), Times.Once);
        myPupilsPresentationStateProvider.Verify(t => t.Set(It.IsAny<MyPupilsPresentationState>()), Times.Once);

        // PupilSelectionState
        pupilSelectionStateProvider.Verify(t => t.GetState(), Times.Once);
        initialState.Verify(t => t.AddPupils(request.ParseCurrentPageOfPupils()), Times.Once);
        initialState.Verify(t => t.SelectAllPupils(), Times.Once);
        pupilSelectionStateProvider.Verify(t => t.SetState(It.IsAny<IMyPupilsPupilSelectionState>()), Times.Once);
    }

    [Fact]
    public void Handle_When_DeselectingAllPupils_DeselectsAllPupils_And_SavesSelectionState()
    {
        // Arrange
        Mock<IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState>> mapper = MapperTestDoubles.Default<MyPupilsFormStateRequestDto, MyPupilsPresentationState>();
        Mock<IMyPupilsPresentationStateProvider> myPupilsPresentationStateProvider = MyPupilsPresentationStateProviderTestDoubles.Default();
        Mock<IPupilSelectionStateProvider> pupilSelectionStateProvider = PupilSelectionStateProviderTestDoubles.Default();
        Mock<IMyPupilsPupilSelectionState> initialState = PupilsSelectionStateTestDoubles.Default();

        MyPupilsFormStateRequestDto request = MyPupilsFormStateRequestDtoTestDoubles.CreateWithSelectionState(selectAll: false);

        mapper.Setup((t) => t.Map(request)).Returns(It.IsAny<MyPupilsPresentationState>());

        initialState.Setup(t => t.AddPupils(It.IsAny<IEnumerable<string>>())).Verifiable();
        pupilSelectionStateProvider.Setup(t => t.GetState()).Returns(initialState.Object);

        UpdateMyPupilsStateCommandHandler sut = new(
            mapper.Object,
            myPupilsPresentationStateProvider.Object,
            pupilSelectionStateProvider.Object);

        // Act
        sut.Handle(request);

        // Assert
        // PresentationState
        mapper.Verify(t => t.Map(request), Times.Once);
        myPupilsPresentationStateProvider.Verify(t => t.Set(It.IsAny<MyPupilsPresentationState>()), Times.Once);

        // PupilSelectionState
        pupilSelectionStateProvider.Verify(t => t.GetState(), Times.Once);
        initialState.Verify(t => t.AddPupils(request.ParseCurrentPageOfPupils()), Times.Once);
        initialState.Verify(t => t.DeselectAllPupils(), Times.Once);
        pupilSelectionStateProvider.Verify(t => t.SetState(It.IsAny<IMyPupilsPupilSelectionState>()), Times.Once);
    }

    [Fact]
    public void Handle_When_NoSelectAll_ManualSelectionApplied_And_SavesSelectionState()
    {
        // Arrange
        Mock<IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState>> mapper = MapperTestDoubles.Default<MyPupilsFormStateRequestDto, MyPupilsPresentationState>();
        Mock<IMyPupilsPresentationStateProvider> myPupilsPresentationStateProvider = MyPupilsPresentationStateProviderTestDoubles.Default();
        Mock<IPupilSelectionStateProvider> pupilSelectionStateProvider = PupilSelectionStateProviderTestDoubles.Default();
        Mock<IMyPupilsPupilSelectionState> initialState = PupilsSelectionStateTestDoubles.Default();

        List<UniquePupilNumber> pageOfPupils = UniquePupilNumberTestDoubles.Generate(20);
        List<UniquePupilNumber> selectedPupilsOnPage = pageOfPupils.Skip(10).ToList();
        MyPupilsFormStateRequestDto request = MyPupilsFormStateRequestDtoTestDoubles.CreateWithSelectedPupils(
            allPupilsOnPage: pageOfPupils,
            selectedPupilsOnPage);

        mapper.Setup((t) => t.Map(request)).Returns(It.IsAny<MyPupilsPresentationState>());

        initialState
            .Setup((state) => state.AddPupils(pageOfPupils.Select(t => t.Value)))
            .Verifiable();

        pupilSelectionStateProvider
            .Setup((t) => t.GetState())
            .Returns(initialState.Object);

        UpdateMyPupilsStateCommandHandler handler = new(
            mapper.Object,
            myPupilsPresentationStateProvider.Object,
            pupilSelectionStateProvider.Object);

        // Act
        handler.Handle(request);

        // Assert
        // PresentationState
        mapper.Verify(t => t.Map(request), Times.Once);
        myPupilsPresentationStateProvider.Verify(t => t.Set(It.IsAny<MyPupilsPresentationState>()), Times.Once);

        // PupilSelectionState
        IEnumerable<string> selectedPupilValuesOnPage = selectedPupilsOnPage.Select(t => t.Value);
        IEnumerable<string> deselectedPupilsValuesOnPage = pageOfPupils.Take(10).Select(t => t.Value);

        pupilSelectionStateProvider.Verify(t => t.GetState(), Times.Once);
        initialState.Verify(t => t.AddPupils(request.ParseCurrentPageOfPupils()), Times.Once);
        initialState.Verify(t => t.UpdatePupilSelectionState(selectedPupilValuesOnPage, true), Times.Once);
        initialState.Verify(t => t.UpdatePupilSelectionState(deselectedPupilsValuesOnPage, false), Times.Once);
        pupilSelectionStateProvider.Verify(t => t.SetState(It.IsAny<IMyPupilsPupilSelectionState>()), Times.Once);
    }
}
