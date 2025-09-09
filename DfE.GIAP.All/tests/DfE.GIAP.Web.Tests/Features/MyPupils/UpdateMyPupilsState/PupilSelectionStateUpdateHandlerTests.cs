using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Features.MyPupils.Routes.UpdateForm;
using DfE.GIAP.Web.Features.MyPupils.Services.UpdateMyPupilsState.PupilSelectionStateUpdater;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;
using DfE.GIAP.Web.Tests.TestDoubles.MyPupils;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.UpdateMyPupilsState;
public sealed class PupilSelectionStateUpdateHandlerTests
{
    [Fact]
    public void Handle_Throws_When_State_Is_Null()
    {
        // Arrange
        List<UniquePupilNumber> currentPage = UniquePupilNumberTestDoubles.Generate(count: 3);
        MyPupilsFormStateRequestDto input = new();

        PupilSelectionStateUpdateHandler sut = new();

        // Act & Assert
        Action act = () => sut.Handle(null!, currentPage, input);

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Handle_Throws_When_CurrentPage_Is_Null()
    {
        // Arrange
        MyPupilsPupilSelectionState state = new();
        MyPupilsFormStateRequestDto input = new();

        PupilSelectionStateUpdateHandler sut = new();

        // Act & Assert
        Action act = () => sut.Handle(state, null!, input);

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Handle_Throws_When_Input_Is_Null()
    {
        // Arrange
        List<UniquePupilNumber> currentPage = UniquePupilNumberTestDoubles.Generate(count: 3);
        MyPupilsPupilSelectionState state = new();

        PupilSelectionStateUpdateHandler sut = new();

        Action act = () => sut.Handle(state, currentPage, null!);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Handle_SelectAll_SelectsAllPupils()
    {
        // Arrange
        List<UniquePupilNumber> currentPage = UniquePupilNumberTestDoubles.Generate(count: 5);
        MyPupilsPupilSelectionState state = new();
        MyPupilsFormStateRequestDto input = new()
        {
            SelectAll = true
        };

        PupilSelectionStateUpdateHandler sut = new();

        // Act
        sut.Handle(state, currentPage, input);

        // Assert
        Assert.All(currentPage, pupil => Assert.True(state.IsPupilSelected(pupil)));
        Assert.True(state.IsAllPupilsSelected);
        Assert.False(state.IsAllPupilsDeselected);
        Assert.True(state.IsAnyPupilSelected);
    }

    [Fact]
    public void Handle_DeselectAll_DeselectsAllPupils()
    {
        // Arrange
        List<UniquePupilNumber> currentPage = UniquePupilNumberTestDoubles.Generate(count: 5);
        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.WithSelectionState(new()
        {
            { currentPage, true }
        });

        MyPupilsFormStateRequestDto input = new()
        {
            SelectAll = false
        };

        PupilSelectionStateUpdateHandler sut = new();

        // Act
        sut.Handle(state, currentPage, input);

        // Assert
        Assert.All(currentPage, pupil => Assert.False(state.IsPupilSelected(pupil)));
        Assert.True(state.IsAllPupilsDeselected);
        Assert.False(state.IsAllPupilsSelected);
        Assert.False(state.IsAnyPupilSelected);
    }

    [Fact]
    public void Handle_ManualSelection_TracksSelectedAndDeselectedPupils()
    {
        // Arrange
        List<UniquePupilNumber> currentPage = UniquePupilNumberTestDoubles.Generate(count: 5);
        UniquePupilNumber[] selected = currentPage.Take(2).ToArray();
        UniquePupilNumber[] deselected = currentPage.Skip(2).ToArray();

        MyPupilsPupilSelectionState state = new();
        MyPupilsFormStateRequestDto input = new()
        {
            SelectedPupils = selected.Select(t => t.Value).ToList()
        };

        PupilSelectionStateUpdateHandler sut = new();

        // Act
        sut.Handle(state, currentPage, input);

        // Assert
        Assert.All(selected, pupil => Assert.True(state.IsPupilSelected(pupil)));
        Assert.All(deselected, pupil => Assert.False(state.IsPupilSelected(pupil)));
        Assert.False(state.IsAllPupilsSelected);
        Assert.False(state.IsAllPupilsDeselected);
        Assert.True(state.IsAnyPupilSelected);
    }
}
