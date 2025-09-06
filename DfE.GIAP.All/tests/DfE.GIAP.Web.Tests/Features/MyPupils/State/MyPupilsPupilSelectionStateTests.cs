﻿using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;
using DfE.GIAP.Web.Tests.TestDoubles.MyPupils;
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
        List<UniquePupilNumber> upns = UniquePupilNumberTestDoubles.Generate(count: 3);

        Dictionary<List<UniquePupilNumber>, bool> selectionStateMapping = new()
        {
            { upns , false }
        };

        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.WithSelectionState(selectionStateMapping);

        // Act
        state.SelectAllPupils();

        // Assert
        Assert.True(state.IsAllPupilsSelected);
        Assert.All(upns, upn => state.IsPupilSelected(upn));
    }

    [Fact]
    public void DeselectAllPupils_Clears_Selections()
    {
        // Arrange
        List<UniquePupilNumber> upns = UniquePupilNumberTestDoubles.Generate(count: 3);

        Dictionary<List<UniquePupilNumber>, bool> selectionStateMapping = new()
        {
            { [upns[0]] , false },
            { [upns[1], upns[2]], true },
        };

        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.WithSelectionState(selectionStateMapping);

        // Act
        state.DeselectAllPupils();

        // Assert
        Assert.True(state.IsAllPupilsDeselected);
        Assert.Equal(3, state.GetPupilsWithSelectionState().Count);
        Assert.All(upns, upn => state.IsPupilSelected(upn));
    }

    [Fact]
    public void ResetState_Clears_All_Data()
    {
        // Arrange
        List<UniquePupilNumber> upns = UniquePupilNumberTestDoubles.Generate(count: 2);

        Dictionary<List<UniquePupilNumber>, bool> selectionStateMapping = new()
        {
            { upns, true }
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
    public void UpsertUniquePupilNumberSelectionState_WithNull_Throws()
    {
        // Arrange Act
        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.Default();

        // Assert
        Assert.Throws<ArgumentNullException>(() => state.UpsertPupilSelectionState(null, It.IsAny<bool>()));
    }

    [Fact]
    public void UpsertUniquePupilNumberSelectionState_WithEmpty_DoesNotAlter()
    {
        // Arrange Act
        // Arrange
        List<UniquePupilNumber> upns = UniquePupilNumberTestDoubles.Generate(count: 3);

        Dictionary<List<UniquePupilNumber>, bool> selectionStateMapping = new()
        {
            { [upns[0]], false },
            { [upns[1]], true }
        };

        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.WithSelectionState(selectionStateMapping);
        IReadOnlyDictionary<UniquePupilNumber, bool> selectionState = state.GetPupilsWithSelectionState();

        // Act
        state.UpsertPupilSelectionState([], It.IsAny<bool>());

        // Assert
        Assert.Equivalent(selectionState, state.GetPupilsWithSelectionState());
    }

    [Fact]
    public void UpsertUniquePupilNumberSelectionState_WithDuplicateUpns_DoesNotDuplicate()
    {
        // Arrange
        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.Default();
        UniquePupilNumber upn = UniquePupilNumberTestDoubles.Generate();
        
        // Act
        state.UpsertPupilSelectionState([upn, upn], true);

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
        UniquePupilNumber upn = UniquePupilNumberTestDoubles.Generate();

        // Act
        state.UpsertPupilSelectionState([upn], selected);


        // Assert
        Assert.Equal(selected, state.IsPupilSelected(upn));
        Assert.Equal(selected, state.GetPupilsWithSelectionState()[upn]);
    }

    [Fact]
    public void UpsertUniquePupilNumberSelectionState_Applies_SelectionStatePerUpn()
    {
        // Arrange
        List<UniquePupilNumber> upns = UniquePupilNumberTestDoubles.Generate(count: 3);

        Dictionary<List<UniquePupilNumber>, bool> selectionStateMapping = new()
        {
            { [upns[1]] , false },
            { [upns[0]] , true }
        };

        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.WithSelectionState(selectionStateMapping);

        // Act
        state.UpsertPupilSelectionState([upns[1]], true);
        state.UpsertPupilSelectionState([upns[0]], false);

        // Assert
        Assert.True(state.IsPupilSelected(upns[1]));
        Assert.False(state.IsPupilSelected(upns[0]));

        IReadOnlyDictionary<UniquePupilNumber, bool> selectionState = state.GetPupilsWithSelectionState();
        Assert.Contains(upns[0], selectionState);
        Assert.Contains(upns[1], selectionState);
    }
}
