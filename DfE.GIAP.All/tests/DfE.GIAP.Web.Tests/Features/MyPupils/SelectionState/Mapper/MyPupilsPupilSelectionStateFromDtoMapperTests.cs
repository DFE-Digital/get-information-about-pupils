/*using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.DataTransferObjects;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.Mapper;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.SelectionState.Mapper;
public sealed class MyPupilsPupilSelectionStateFromDtoMapperTests
{
    [Fact]
    public void Map_Throws_When_Input_Is_Null()
    {
        // Arrange
        MyPupilsPupilSelectionStateFromDtoMapper sut = new();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => sut.Map(null!));
    }

    [Fact]
    public void Map_Sets_SelectAll_State_And_Selection()
    {
        // Arrange
        List<UniquePupilNumber> pupils = UniquePupilNumberTestDoubles.Generate(count: 3);
        Dictionary<string, bool> selectionMap = pupils.ToDictionary(p => p.Value, _ => true);

        MyPupilsPupilSelectionStateDto dto = new()
        {
            State = PupilSelectionModeDto.SelectAll,
            PupilUpnToSelectedMap = selectionMap
        };

        MyPupilsPupilSelectionStateFromDtoMapper sut = new();

        // Act
        MyPupilsPupilSelectionState result = sut.Map(dto);

        // Assert
        Assert.True(result.IsAllPupilsSelected);
        Assert.True(result.IsAnyPupilSelected);
        Assert.All(pupils, pupil => Assert.True(result.IsPupilSelected(pupil.Value)));
    }

    [Fact]
    public void Map_Sets_DeselectAll_State_And_Selection()
    {
        // Arrange
        List<UniquePupilNumber> pupils = UniquePupilNumberTestDoubles.Generate(count: 3);
        Dictionary<string, bool> selectionMap = pupils.ToDictionary(p => p.Value, _ => false);

        MyPupilsPupilSelectionStateDto dto = new()
        {
            State = PupilSelectionModeDto.DeselectAll,
            PupilUpnToSelectedMap = selectionMap
        };

        MyPupilsPupilSelectionStateFromDtoMapper sut = new();

        // Act
        MyPupilsPupilSelectionState result = sut.Map(dto);

        // Assert
        Assert.False(result.IsAllPupilsSelected);
        Assert.False(result.IsAnyPupilSelected);
        Assert.All(pupils, pupil => Assert.False(result.IsPupilSelected(pupil.Value)));
    }

    [Fact]
    public void Map_Sets_Mixed_Selection_When_State_Is_NotSpecified()
    {
        // Arrange
        List<UniquePupilNumber> pupils = UniquePupilNumberTestDoubles.Generate(count: 4);
        Dictionary<string, bool> selectionMap = new()
        {
            { pupils[0].Value, true },
            { pupils[1].Value, true },
            { pupils[2].Value, false },
            { pupils[3].Value, false }
        };

        MyPupilsPupilSelectionStateDto dto = new()
        {
            State = PupilSelectionModeDto.ManualSelection,
            PupilUpnToSelectedMap = selectionMap
        };

        MyPupilsPupilSelectionStateFromDtoMapper sut = new();

        // Act
        MyPupilsPupilSelectionState result = sut.Map(dto);

        // Assert
        Assert.False(result.IsAllPupilsSelected);
        Assert.True(result.IsAnyPupilSelected);
        Assert.True(result.IsPupilSelected(pupils[0].Value));
        Assert.True(result.IsPupilSelected(pupils[1].Value));
        Assert.False(result.IsPupilSelected(pupils[2].Value));
        Assert.False(result.IsPupilSelected(pupils[3].Value));
    }
}
*/
