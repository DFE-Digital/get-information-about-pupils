using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;
using DfE.GIAP.Web.Tests.Features.MyPupils.TestDoubles;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.State;
public sealed class MyPupilsPupilSelectionStateTests
{
    [Fact]
    public void Default_State_Is_Empty()
    {
        // Arrange Act
        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.Default();

        // Assert
        Assert.False(state.IsAllPupilsSelected);
        Assert.Empty(state.GetPupilsWithSelectionState());
    }

    [Fact]
    public void SelectAllPupils_Updates_State_WithEmptyPupils()
    {
        // Arrange
        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.Default();

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

        Dictionary<IEnumerable<string>, bool> selectionStateMapping = new()
        {
            { upns , false }
        };

        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.WithSelectionState(selectionStateMapping);

        // Act
        state.SelectAllPupils();

        // Assert
        Assert.True(state.IsAllPupilsSelected);
        Assert.All(upns, upn => Assert.True(state.IsPupilSelected(upn)));
    }

    [Fact]
    public void DeselectAllPupils_Clears_Selections()
    {
        // Arrange
        List<string> upns = UniquePupilNumberTestDoubles.GenerateAsValues(count: 3);

        Dictionary<IEnumerable<string>, bool> selectionStateMapping = new()
        {
            { [upns[0]] , false },
            { [upns[1]] , true },
            { [upns[2]] , true },
        };

        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.WithSelectionState(selectionStateMapping);

        // Act
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

        Dictionary<IEnumerable<string>, bool> selectionStateMapping = new()
        {
            { upns , true }
        };

        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.WithSelectionState(selectionStateMapping);

        // Act
        state.ResetState();

        // Assert
        Assert.False(state.IsAllPupilsSelected);
        Assert.Empty(state.GetPupilsWithSelectionState());
        Assert.All(upns, upn => Assert.False(state.IsPupilSelected(upn)));
    }

    [Fact]
    public void UpsertUniquePupilNumberSelectionState_WithNullOrEmpty_Throws()
    {
        // Arrange Act
        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.Default();

        // Assert
        Assert.Throws<ArgumentNullException>(() => state.UpsertUniquePupilNumberSelectionState(null, It.IsAny<bool>()));
        Assert.Throws<ArgumentException>(() => state.UpsertUniquePupilNumberSelectionState([], It.IsAny<bool>()));
    }

    [Fact]
    public void UpsertUniquePupilNumberSelectionState_WithDuplicateUpns_DoesNotDuplicate()
    {
        // Arrange
        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.Default();
        string upn = UniquePupilNumberTestDoubles.Generate().Value;

        // Act
        state.UpsertUniquePupilNumberSelectionState([upn, upn], true);

        // Assert
        Assert.Single(state.GetPupilsWithSelectionState());
        Assert.True(state.IsPupilSelected(upn));
    }


    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void UpsertUniquePupilNumberSelectionState_OnUnknownButValidUpn_Adds(bool selected)
    {
        // Arrange
        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.Default();
        string upn = UniquePupilNumberTestDoubles.Generate().Value;

        // Act
        state.UpsertUniquePupilNumberSelectionState([upn], selected);


        // Assert
        Assert.Equal(selected, state.IsPupilSelected(upn));
        Assert.Equal(selected, state.GetPupilsWithSelectionState()[upn]);
    }

    [Fact]
    public void UpsertUniquePupilNumberSelectionState_Applies_SelectionStatePerUpn()
    {
        // Arrange
        List<string> upns = UniquePupilNumberTestDoubles.GenerateAsValues(count: 3);

        Dictionary<IEnumerable<string>, bool> selectionStateMapping = new()
        {
            { [upns[0]] , false },
            { [upns[1]] , true }
        };

        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.WithSelectionState(selectionStateMapping);

        // Act
        state.UpsertUniquePupilNumberSelectionState([upns[0]], true);
        state.UpsertUniquePupilNumberSelectionState([upns[1]], false);

        // Assert
        Assert.True(state.IsPupilSelected(upns[0]));
        Assert.False(state.IsPupilSelected(upns[1]));
        IReadOnlyDictionary<string, bool> selectionState = state.GetPupilsWithSelectionState();
        Assert.Contains(upns[0], selectionState);
        Assert.Contains(upns[1], selectionState);
    }

    [Theory]
    [MemberData(nameof(InvalidUpns))]
    public void UpsertUniquePupilNumberSelectionState_WithInvalidUpns_Throws(List<string> upns)
    {
        // Arrange
        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.Default();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => state.UpsertUniquePupilNumberSelectionState(upns, It.IsAny<bool>()));
    }

#pragma warning disable S3878 // Arrays should not be created for params parameters Note: Test data
    public static TheoryData<List<string>> InvalidUpns => new(
        ["INVALID-1"],
        ["INVALID-1", "INVALID-2"],
        [UniquePupilNumberTestDoubles.Generate().Value, "INVALID_UPN"]);
#pragma warning restore S3878 // Arrays should not be created for params parameters
}
