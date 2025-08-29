using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;
using DfE.GIAP.Web.Tests.Controllers.MyPupilList.TestDoubles;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.MyPupilList;
public sealed class PupilSelectionStateTests
{
    [Fact]
    public void Default_State_Is_Empty()
    {
        // Arrange Act
        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.CreateEmpty();

        // Assert
        Assert.False(state.IsAllPupilsSelected);
        Assert.Empty(state.GetPupilsWithSelectionState());
    }

    [Fact]
    public void SelectAllPupils_Updates_State_WithEmptyPupils()
    {
        // Arrange
        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.CreateEmpty();

        // Act
        state.SelectAllPupils();

        // Assert
        Assert.True(state.IsAllPupilsSelected);
        Assert.Empty(state.GetPupilsWithSelectionState());
    }

    [Fact]
    public void SelectAllPupils_Updates_State_WithSomePupils()
    {
        // Arrange
        List<string> upns = UniquePupilNumberTestDoubles.GenerateAsValues(count: 3);
        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.CreateWithSelectionState(upns);

        // Act
        state.SelectAllPupils();

        // Assert
        Assert.True(state.IsAllPupilsSelected);
        Assert.All(upns, upn => Assert.True(state.IsPupilSelected(upn)));
    }

    [Fact]
    public void UpdateSelectionState_SelectsAndDeselectsPupils()
    {
        // Arrange
        List<string> upns = UniquePupilNumberTestDoubles.GenerateAsValues(count: 3);
        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.CreateWithSelectionState(upns);

        // Act
        state.UpsertPupilWithSelectedState([upns[0]], true);
        state.UpsertPupilWithSelectedState([upns[1]], false);

        // Assert
        Assert.True(state.IsPupilSelected(upns[0]));
        Assert.False(state.IsPupilSelected(upns[1]));
        IReadOnlyDictionary<string, bool> selectionState = state.GetPupilsWithSelectionState();
        Assert.Contains(upns[0], selectionState);
        Assert.Contains(upns[1], selectionState);
    }

    [Fact]
    public void DeselectAllPupils_Clears_Selections()
    {
        // Arrange
        List<string> upns = UniquePupilNumberTestDoubles.GenerateAsValues(count: 3);
        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.CreateWithSelectionState(upns);

        // Act
        state.SelectAllPupils();
        state.DeselectAllPupils();

        // Assert
        Assert.False(state.IsAllPupilsSelected);
        Assert.Equal(3, state.GetPupilsWithSelectionState().Count);
        Assert.All(upns, upn => Assert.False(state.IsPupilSelected(upn)));
    }

    [Fact]
    public void ResetState_Clears_All_Data()
    {
        // Arrange
        List<string> upns = UniquePupilNumberTestDoubles.GenerateAsValues(count: 2);
        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.CreateWithSelectionState(upns);
        string selectedUpn = upns[0];
        state.UpsertPupilWithSelectedState([selectedUpn], true);

        // Act
        state.ResetState();

        // Assert
        Assert.False(state.IsAllPupilsSelected);
        Assert.Empty(state.GetPupilsWithSelectionState());
        Assert.All(upns, upn => Assert.False(state.IsPupilSelected(upn)));
    }

    [Fact]
    public void AddPupils_WithNullOrEmpty_Throws()
    {
        // Arrange Act
        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.CreateEmpty();

        // Assert
        Assert.Throws<ArgumentNullException>(() => state.UpsertPupilWithSelectedState(null, It.IsAny<bool>()));
        Assert.Throws<ArgumentException>(() => state.UpsertPupilWithSelectedState([], It.IsAny<bool>()));
    }

    [Fact]
    public void AddPupils_WithInvalidUpn_Throws()
    {
        // Arrange
        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.CreateEmpty();
        List<string> invalidUpns = ["INVALID_UPN"];

        // Act & Assert
        ArgumentException ex = Assert.Throws<ArgumentException>(() => state.UpsertPupilWithSelectedState(invalidUpns, It.IsAny<bool>()));
        Assert.Equal("Invalid UPN requested", ex.Message);
    }


    [Fact]
    public void AddPupils_WithDuplicateUpns_DoesNotThrowOrDuplicate()
    {
        // Arrange
        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.CreateEmpty();
        string upn = UniquePupilNumberTestDoubles.Generate().Value;

        // Act
        state.UpsertPupilWithSelectedState([upn, upn], true);

        // Assert
        Assert.Single(state.GetPupilsWithSelectionState());
        Assert.True(state.IsPupilSelected(upn));
    }


    [Fact]
    public void UpdateSelectionState_OnUnknownUpn_AddsAndMarksSelected()
    {
        // Arrange
        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.CreateEmpty();
        string upn = UniquePupilNumberTestDoubles.Generate().Value;

        // Act
        state.UpsertPupilWithSelectedState([upn], true);

        // Assert
        Assert.True(state.IsPupilSelected(upn));
        Assert.True(state.GetPupilsWithSelectionState()[upn]);
    }

    [Fact]
    public void UpdateSelectionState_OnUnknownUpn_AddsAndMarksDeselected()
    {
        // Arrange
        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.CreateEmpty();
        string upn = UniquePupilNumberTestDoubles.Generate().Value;

        // Act
        state.UpsertPupilWithSelectedState([upn], false);


        // Assert
        Assert.False(state.IsPupilSelected(upn));
        Assert.False(state.GetPupilsWithSelectionState()[upn]);
    }

    [Fact]
    public void UpdateSelectionState_WithInvalidUpn_Throws()
    {
        // Arrange
        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.CreateEmpty();
        List<string> invalidUpns = ["INVALID-1", "INVALID-2"];

        // Act & Assert
        Assert.Throws<ArgumentException>(() => state.UpsertPupilWithSelectedState(invalidUpns, It.IsAny<bool>()));
    }

    [Fact]
    public void UpdateSelectionState_WithMixedValidAndInvalidUpns_Throws()
    {
        // Arrange
        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.CreateEmpty();
        string validUpn = UniquePupilNumberTestDoubles.Generate().Value;
        List<string> mixedUpns = [validUpn, "INVALID_UPN"];

        // Act & Assert
        Assert.Throws<ArgumentException>(() => state.UpsertPupilWithSelectedState(mixedUpns, It.IsAny<bool>()));
    }
}
