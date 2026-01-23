using DfE.GIAP.Web.Features.MyPupils.PupilSelection.Operations;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.Operations.Mapper;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.Operations.Mapper.DataTransferObjects;
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
    public void Map_SelectAllMode_SelectsAllPupils_And_Ignores_Null_DeselectedExceptions()
    {
        // Arrange
        MyPupilsPupilSelectionStateDto dto = new()
        {
            Mode = SelectionMode.All,
            DeselectedExceptions = null
        };

        MyPupilsPupilSelectionStateFromDtoMapper sut = new();

        // Act
        MyPupilsPupilSelectionState response = sut.Map(dto);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(SelectionMode.All, response.Mode);
        Assert.Empty(response.GetManualSelections());
        Assert.Empty(response.GetDeselectedExceptions());
    }

    [Fact]
    public void Map_SelectAllMode_SelectsAllPupils_Except_EmptyOrNull_DeselectedExceptions()
    {
        // Arrange
        MyPupilsPupilSelectionStateDto dto = new()
        {
            Mode = SelectionMode.All,
            DeselectedExceptions = [null, string.Empty, " ", "\n", "a"]
        };

        MyPupilsPupilSelectionStateFromDtoMapper sut = new();

        // Act
        MyPupilsPupilSelectionState response = sut.Map(dto);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(SelectionMode.All, response.Mode);

        List<string> expectedDeselections = ["a"];
        Assert.Empty(response.GetManualSelections());
        Assert.Equivalent(expectedDeselections, response.GetDeselectedExceptions());
        Assert.True(response.IsAnyPupilSelected);
    }

    [Fact]
    public void Map_ManualSelectMode_Ignores_Null_ExplicitSelections()
    {
        // Arrange
        MyPupilsPupilSelectionStateDto dto = new()
        {
            Mode = SelectionMode.Manual,
            ExplicitSelections = null
        };

        MyPupilsPupilSelectionStateFromDtoMapper sut = new();

        // Act
        MyPupilsPupilSelectionState response = sut.Map(dto);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(SelectionMode.Manual, response.Mode);

        Assert.Empty(response.GetManualSelections());
        Assert.Empty(response.GetDeselectedExceptions());
        Assert.False(response.IsAnyPupilSelected);
    }

    [Fact]
    public void Map_ManualSelectMode_Selects_ManualSelections()
    {
        // Arrange
        MyPupilsPupilSelectionStateDto dto = new()
        {
            Mode = SelectionMode.Manual,
            ExplicitSelections = [null, string.Empty, " ", "\n", "a"]
        };

        MyPupilsPupilSelectionStateFromDtoMapper sut = new();

        // Act
        MyPupilsPupilSelectionState response = sut.Map(dto);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(SelectionMode.Manual, response.Mode);

        List<string> selections = ["a"];
        Assert.Equivalent(selections, response.GetManualSelections());
        Assert.Empty(response.GetDeselectedExceptions());
        Assert.True(response.IsAnyPupilSelected);
    }
}

