using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Web.Features.Search.Shared.Filters.Handlers;
using DfE.GIAP.Web.ViewModels.Search;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.Search.Shared.Filters.Handlers;

public class NameFilterHandlerTests
{
    private const string TargetKey = "surname";
    private readonly NameFilterHandler _handler = new(TargetKey);

    [Fact]
    public void Apply_WithValidName_AddsToRequestFilters()
    {
        // arrange
        LearnerTextSearchViewModel model = new();
        Dictionary<string, string[]> requestFilters = [];
        CurrentFilterDetail filter = new()
        {
            FilterName = "Smith",
            FilterType = FilterType.Surname
        };

        // act
        _handler.Apply(filter, model, requestFilters);

        // assert
        Assert.True(requestFilters.ContainsKey(TargetKey));
        Assert.Single(requestFilters[TargetKey]);
        Assert.Equal("Smith", requestFilters[TargetKey][0]);
    }

    [Fact]
    public void Apply_WithDuplicateName_DoesNotAddDuplicate()
    {
        // arrange
        LearnerTextSearchViewModel model = new();
        Dictionary<string, string[]> requestFilters = new()
        {
            { TargetKey, ["Smith"] }
        };

        CurrentFilterDetail filter = new()
        {
            FilterName = "Smith",
            FilterType = FilterType.Surname
        };

        // act
        _handler.Apply(filter, model, requestFilters);

        // assert
        Assert.Single(requestFilters[TargetKey]);
        Assert.Equal("Smith", requestFilters[TargetKey][0]);
    }

    [Fact]
    public void Apply_WithNewName_AppendsToExistingList()
    {
        // arrange
        LearnerTextSearchViewModel model = new();
        Dictionary<string, string[]> requestFilters = new()
        {
            { TargetKey, ["Smith"] }
        };
        CurrentFilterDetail filter = new()
        {
            FilterName = "Jones",
            FilterType = FilterType.Surname
        };

        // act
        _handler.Apply(filter, model, requestFilters);

        // assert
        Assert.Equal(2, requestFilters[TargetKey].Length);
        Assert.Contains("Smith", requestFilters[TargetKey]);
        Assert.Contains("Jones", requestFilters[TargetKey]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Apply_WithEmptyOrWhitespaceName_DoesNothing(string? input)
    {
        // arrange
        LearnerTextSearchViewModel model = new();
        Dictionary<string, string[]> requestFilters = [];
        CurrentFilterDetail filter = new()
        {
            FilterName = input,
            FilterType = FilterType.Surname
        };

        // act
        _handler.Apply(filter, model, requestFilters);

        // assert
        Assert.Empty(requestFilters);
    }
}

