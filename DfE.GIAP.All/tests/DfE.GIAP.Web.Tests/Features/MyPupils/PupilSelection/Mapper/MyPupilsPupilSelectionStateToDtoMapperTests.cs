using DfE.GIAP.Web.Features.MyPupils.PupilSelection;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.Mapper;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.Mapper.DataTransferObjects;
using DfE.GIAP.Web.Tests.Features.MyPupils.TestDoubles;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.PupilSelection.Mapper;
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
    public void Map_SelectAllMode_Stores_DeselectedExceptions()
    {
        // Arrange
        List<string> deselectedExceptions = ["c", "d"];

        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.WithSelectedPupils(
            SelectionMode.All,
            selected: ["a", "b"],
            deselected: deselectedExceptions);

        MyPupilsPupilSelectionStateToDtoMapper sut = new();

        // Act
        MyPupilsPupilSelectionStateDto response = sut.Map(state);

        // Assert
        Assert.NotNull(response);
        Assert.Equivalent(deselectedExceptions, response.DeselectedExceptions);
        Assert.Empty(response.ExplicitSelections);
    }

    [Fact]
    public void Map_ManualMode_Stores_ExplicitSelections()
    {
        // Arrange
        List<string> selected = ["a", "b"];

        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.WithSelectedPupils(
            SelectionMode.Manual,
            selected: selected,
            deselected: ["c", "d"]);

        MyPupilsPupilSelectionStateToDtoMapper sut = new();

        // Act
        MyPupilsPupilSelectionStateDto response = sut.Map(state);

        // Assert
        Assert.NotNull(response);
        Assert.Equivalent(selected, response.ExplicitSelections);
        Assert.Empty(response.DeselectedExceptions);

    }
}

