using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Web.Controllers.TextBasedSearch.Filters.Handlers;
using DfE.GIAP.Web.ViewModels.Search;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.Search.TextBasedSearch.Filters.Handlers;

public class GenderFilterHandlerTests
{
    private const string TargetKey = "Gender";
    private readonly GenderFilterHandler _handler = new(TargetKey);

    [Fact]
    public void Apply_WithNewFilter_AddsInitialToModelAndRequestFilters()
    {
        // arrange
        LearnerTextSearchViewModel model = new();
        Dictionary<string, string[]> requestFilters = [];
        CurrentFilterDetail filter = new()
        {
            FilterName = "Male",
            FilterType = FilterType.Gender
        };

        // act
        _handler.Apply(filter, model, requestFilters);

        // assert
        FilterData genderFilter =
            model.Filters.Single(filterData => filterData.Name == TargetKey);

        Assert.Single(genderFilter.Items);
        Assert.Equal("M", genderFilter.Items[0].Value);

        Assert.True(requestFilters.ContainsKey(TargetKey));
        Assert.Equal("M", requestFilters[TargetKey][0]);
    }

    [Fact]
    public void Apply_WithExistingFilter_DoesNotAddDuplicateInitial()
    {
        // arrange
        LearnerTextSearchViewModel model = new()
        {
            Filters =
            [
                new FilterData
                {
                    Name = TargetKey,
                    Items = [new FilterDataItem { Value = "F" }]
                }
            ]
        };
        Dictionary<string, string[]> requestFilters = [];
        CurrentFilterDetail filter = new()
        {
            FilterName = "Female",
            FilterType = FilterType.Gender
        };

        // act
        _handler.Apply(filter, model, requestFilters);

        // assert
        FilterData genderFilter =
            model.Filters.Single(filterData => filterData.Name == TargetKey);

        Assert.Single(genderFilter.Items); // No duplicate added
        Assert.Equal("F", genderFilter.Items[0].Value);

        Assert.True(requestFilters.ContainsKey(TargetKey));
        Assert.Equal("F", requestFilters[TargetKey][0]);
    }

    [Fact]
    public void Apply_WithNewInitial_AppendsToExistingFilterData()
    {
        // arrange
        LearnerTextSearchViewModel model = new()
        {
            Filters =
            [
                new FilterData
                {
                    Name = TargetKey,
                    Items = [new FilterDataItem { Value = "F" }]
                }
            ]
        };
        Dictionary<string, string[]> requestFilters = [];
        CurrentFilterDetail filter = new()
        {
            FilterName = "Male",
            FilterType = FilterType.Gender
        };

        // act
        _handler.Apply(filter, model, requestFilters);

        // assert
        FilterData genderFilter =
            model.Filters.Single(filterData => filterData.Name == TargetKey);

        Assert.Equal(2, genderFilter.Items.Count);
        Assert.Contains(genderFilter.Items, i => i.Value == "F");
        Assert.Contains(genderFilter.Items, i => i.Value == "M");

        Assert.True(requestFilters.ContainsKey(TargetKey));
        Assert.Equal(new[] { "F", "M" }, requestFilters[TargetKey]);
    }

    [Fact]
    public void Apply_WithEmptyModelFilters_InitializesAndAddsEntry()
    {
        // arrange
        LearnerTextSearchViewModel model = new() { Filters = null };
        Dictionary<string, string[]> requestFilters = [];
        CurrentFilterDetail filter = new()
        {
            FilterName = "Male",
            FilterType = FilterType.Gender
        };

        // act
        _handler.Apply(filter, model, requestFilters);

        // assert
        Assert.NotNull(model.Filters);
        Assert.Single(model.Filters);
        Assert.Equal("M", model.Filters[0].Items[0].Value);
        Assert.Equal("M", requestFilters[TargetKey][0]);
    }
}

