using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Controllers.MyPupilList.PupilSelectionState;
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
        MyPupilsPupilSelectionState state = new();

        // Assert
        Assert.False(state.IsAllPupilsSelected);
        Assert.Empty(state.GetSelectedPupils());
    }

    [Fact]
    public void SelectAllPupils_Updates_State_WithEmptyPupils()
    {
        // Arrange
        MyPupilsPupilSelectionState state = new();

        // Act
        state.SelectAllPupils();

        // Assert
        Assert.True(state.IsAllPupilsSelected);
        Assert.Empty(state.GetSelectedPupils());
    }

    [Fact]
    public void SelectAllPupils_Updates_State_WithSomePupils()
    {
        // Arrange
        List<string> upns = UniquePupilNumberTestDoubles.GenerateAsValues(count: 3);
        MyPupilsPupilSelectionState state = PupilsSelectionStateTestDoubles.CreateWithPupilUniquePupilNumbers(upns);

        // Act
        state.SelectAllPupils();

        // Assert
        Assert.True(state.IsAllPupilsSelected);
        Assert.Equivalent(upns, state.GetSelectedPupils());
        Assert.All(upns, upn => Assert.True(state.IsPupilSelected(upn)));
    }

    [Fact]
    public void UpdateSelectionState_SelectsAndDeselectsPupils()
    {
        // Arrange
        List<string> upns = UniquePupilNumberTestDoubles.GenerateAsValues(count: 3);
        MyPupilsPupilSelectionState state = PupilsSelectionStateTestDoubles.CreateWithPupilUniquePupilNumbers(upns);

        // Act
        state.UpdatePupilSelectionState([upns[0]], true);
        state.UpdatePupilSelectionState([upns[1]], false);

        // Assert
        Assert.True(state.IsPupilSelected(upns[0]));
        Assert.False(state.IsPupilSelected(upns[1]));
        Assert.Contains(upns[0], state.GetSelectedPupils());
        Assert.DoesNotContain(upns[1], state.GetSelectedPupils());
    }

    [Fact]
    public void DeselectAllPupils_Clears_Selections()
    {
        // Arrange
        List<string> upns = UniquePupilNumberTestDoubles.GenerateAsValues(count: 3);
        MyPupilsPupilSelectionState state = PupilsSelectionStateTestDoubles.CreateWithPupilUniquePupilNumbers(upns);

        // Act
        state.SelectAllPupils();
        state.DeselectAllPupils();

        // Assert
        Assert.False(state.IsAllPupilsSelected);
        Assert.Empty(state.GetSelectedPupils());
        Assert.All(upns, upn => Assert.False(state.IsPupilSelected(upn)));
    }

    [Fact]
    public void ResetState_Clears_All_Data()
    {
        // Arrange
        List<string> upns = UniquePupilNumberTestDoubles.GenerateAsValues(count: 2);
        MyPupilsPupilSelectionState state = PupilsSelectionStateTestDoubles.CreateWithPupilUniquePupilNumbers(upns);
        string selectedUpn = upns[0];
        state.UpdatePupilSelectionState([selectedUpn], true);

        // Act
        state.ClearPupilsAndState();

        // Assert
        Assert.False(state.IsAllPupilsSelected);
        Assert.Empty(state.GetSelectedPupils());
        Assert.All(upns, upn => Assert.False(state.IsPupilSelected(upn)));
    }

    [Fact]
    public void AddPupils_WithNullUpns_Throws()
    {
        // Arrange Act
        MyPupilsPupilSelectionState state = new();

        // Assert
        Assert.Throws<ArgumentNullException>(() => state.AddPupils(null));
    }

    [Fact]
    public void AddPupils_WithEmptyUpns_DoesNotThrows()
    {
        // Arrange Act
        MyPupilsPupilSelectionState state = new();

        // Assert
        state.AddPupils([]);
        Assert.False(state.IsAllPupilsSelected);
        Assert.Empty(state.GetSelectedPupils());
    }

    [Fact]
    public void AddPupils_WithInvalidUpn_Throws()
    {
        // Arrange
        MyPupilsPupilSelectionState state = new();
        List<string> invalidUpns = ["INVALID_UPN"];

        // Act & Assert
        ArgumentException ex = Assert.Throws<ArgumentException>(() => state.AddPupils(invalidUpns));
        Assert.Equal("Invalid UPN requested", ex.Message);
    }


    [Fact]
    public void AddPupils_WithDuplicateUpns_DoesNotThrowOrDuplicate()
    {
        // Arrange
        MyPupilsPupilSelectionState state = new();
        string upn = UniquePupilNumberTestDoubles.Generate().Value;
        state.AddPupils([upn, upn]);

        // Act
        state.UpdatePupilSelectionState([upn], true);

        // Assert
        Assert.Single(state.GetSelectedPupils());
        Assert.True(state.IsPupilSelected(upn));
    }


    [Fact]
    public void UpdateSelectionState_OnUnknownUpn_AddsAndMarksSelected()
    {
        // Arrange
        MyPupilsPupilSelectionState state = new();
        string upn = UniquePupilNumberTestDoubles.Generate().Value;

        // Act
        state.UpdatePupilSelectionState([upn], true);

        // Assert
        Assert.True(state.IsPupilSelected(upn));
        Assert.Contains(upn, state.GetSelectedPupils());
    }

    [Fact]
    public void UpdateSelectionState_OnUnknownUpn_AddsAndMarksDeselected()
    {
        // Arrange
        MyPupilsPupilSelectionState state = new();
        string upn = UniquePupilNumberTestDoubles.Generate().Value;

        // Act
        state.UpdatePupilSelectionState([upn], false);


        // Assert
        Assert.False(state.IsPupilSelected(upn));
        Assert.DoesNotContain(upn, state.GetSelectedPupils());
    }

    [Fact]
    public void UpdateSelectionState_WithInvalidUpn_Throws()
    {
        // Arrange
        MyPupilsPupilSelectionState state = new();
        List<string> invalidUpns = ["INVALID-1", "INVALID-2"];

        // Act & Assert
        Assert.Throws<ArgumentException>(() => state.UpdatePupilSelectionState(invalidUpns, It.IsAny<bool>()));
    }

    [Fact]
    public void UpdateSelectionState_WithMixedValidAndInvalidUpns_Throws()
    {
        // Arrange
        MyPupilsPupilSelectionState state = new();
        string validUpn = UniquePupilNumberTestDoubles.Generate().Value;
        List<string> mixedUpns = [validUpn, "INVALID_UPN"];

        // Act & Assert
        Assert.Throws<ArgumentException>(() => state.UpdatePupilSelectionState(mixedUpns, It.IsAny<bool>()));
    }

    //[Fact]
    //public void ToDto_And_FromDto_Preserves_State()
    //{
    //    // Arrange
    //    List<string> upns = UniquePupilNumberTestDoubles.GenerateAsValues(count: 2);
    //    MyPupilsPupilSelectionState original = new();
    //    original.AddPupils(upns);
    //    original.UpdatePupilSelectionState([upns[0]], true);
    //    original.SelectAllPupils();

    //    // Act
    //    MyPupilsPupilSelectionStateDto dto = original.ToDto();
    //    MyPupilsPupilSelectionState restored = MyPupilsPupilSelectionState.FromDto(dto);

    //    // Assert
    //    Assert.True(restored.IsAllPupilsSelected);
    //    Assert.Equivalent(original.GetSelectedPupils(), restored.GetSelectedPupils());
    //    Assert.All(upns, upn => Assert.True(restored.IsPupilSelected(upn)));
    //}

    //[Fact]
    //public void FromDto_WithEmptyMapAndUnspecifiedState_CreatesEmptyState()
    //{
    //    // Arrange
    //    MyPupilsPupilSelectionStateDto dto = new()
    //    {
    //        PupilUpnToSelectedMap = [],
    //        State = SelectAllPupilsState.NotSpecified
    //    };

    //    MyPupilsPupilSelectionState state = MyPupilsPupilSelectionState.FromDto(dto);

    //    Assert.False(state.IsAllPupilsSelected);
    //    Assert.Empty(state.GetSelectedPupils());
    //}
}
