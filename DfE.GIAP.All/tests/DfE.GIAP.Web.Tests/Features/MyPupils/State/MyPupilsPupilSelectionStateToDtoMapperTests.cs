using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;
using DfE.GIAP.Web.Features.MyPupils.State.Selection.DataTransferObjects;
using DfE.GIAP.Web.Features.MyPupils.State.Selection.Mapper;
using DfE.GIAP.Web.Tests.TestDoubles.MyPupils;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.State;
public sealed class MyPupilsPupilSelectionStateToDtoMapperTests
{
    [Fact]
    public void Map_Throws_When_Source_Is_Null()
    {
        // Arrange
        MyPupilsPupilSelectionStateToDtoMapper sut = new();

        // Act Assert
        Assert.Throws<ArgumentNullException>(() => sut.Map(null!));
    }

    [Fact]
    public void Map_Returns_SelectAll_When_AllPupilsSelected()
    {
        // Arrange
        List<string> pupils = UniquePupilNumberTestDoubles.Generate(count: 3)
            .Select(t => t.Value)
            .ToList();

        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.Default();
        state.UpsertPupilSelectionState(pupils, isSelected: true);
        state.SelectAllPupils();

        MyPupilsPupilSelectionStateToDtoMapper sut = new();

        // Act
        MyPupilsPupilSelectionStateDto result = sut.Map(state);

        // Assert
        Assert.Equal(PupilSelectionModeDto.SelectAll, result.State);
        Assert.Equal(pupils.Count, result.PupilUpnToSelectedMap.Count);
        Assert.All(pupils, pupil => Assert.True(result.PupilUpnToSelectedMap[pupil]));
    }

    [Fact]
    public void Map_Returns_DeselectAll_When_AllPupilsDeselected()
    {
        // Arrange
        List<string> pupils = UniquePupilNumberTestDoubles.Generate(count: 3)
            .Select(t => t.Value)
            .ToList();

        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.Default();
        state.UpsertPupilSelectionState(pupils, isSelected: false);
        state.DeselectAllPupils();

        MyPupilsPupilSelectionStateToDtoMapper sut = new();

        // Act
        MyPupilsPupilSelectionStateDto result = sut.Map(state);

        // Assert
        Assert.Equal(PupilSelectionModeDto.DeselectAll, result.State);
        Assert.Equal(pupils.Count, result.PupilUpnToSelectedMap.Count);
        Assert.All(pupils, pupil => Assert.False(result.PupilUpnToSelectedMap[pupil]));
    }

    [Fact]
    public void Map_Returns_NotSpecified_When_SelectionIsMixed()
    {
        // Arrange
        List<string> pupils = UniquePupilNumberTestDoubles.Generate(count: 4)
            .Select(t => t.Value)
            .ToList();

        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.Default();
        state.UpsertPupilSelectionState([pupils[0], pupils[1]], isSelected: true);
        state.UpsertPupilSelectionState([pupils[2], pupils[3]], isSelected: false);

        MyPupilsPupilSelectionStateToDtoMapper sut = new();

        // Act
        MyPupilsPupilSelectionStateDto result = sut.Map(state);

        // Assert
        Assert.Equal(PupilSelectionModeDto.ManualSelection, result.State);
        Assert.Equal(4, result.PupilUpnToSelectedMap.Count);
        Assert.True(result.PupilUpnToSelectedMap[pupils[0]]);
        Assert.True(result.PupilUpnToSelectedMap[pupils[1]]);
        Assert.False(result.PupilUpnToSelectedMap[pupils[2]]);
        Assert.False(result.PupilUpnToSelectedMap[pupils[3]]);
    }
}
