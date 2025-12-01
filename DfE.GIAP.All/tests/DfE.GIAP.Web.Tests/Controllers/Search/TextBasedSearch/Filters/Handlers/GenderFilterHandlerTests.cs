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
            FilterName = "John",
            FilterType = FilterType.Forename
        };

        // act
        _handler.Apply(filter, model, requestFilters);

        // assert
        FilterData genderFilter =
            model.Filters.Single(filterData => filterData.Name == TargetKey);

        Assert.Single(genderFilter.Items);
        Assert.Equal("John", genderFilter.Items[0].Value);

        Assert.True(requestFilters.ContainsKey(TargetKey));
        Assert.Equal("John", requestFilters[TargetKey][0]);
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
                    Items = [new FilterDataItem { Value = "John" }]
                }
            ]
        };
        Dictionary<string, string[]> requestFilters = [];
        CurrentFilterDetail filter = new()
        {
            FilterName = "John",
            FilterType = FilterType.Forename
        };

        // act
        _handler.Apply(filter, model, requestFilters);

        // assert
        FilterData filterData =
            model.Filters.Single((filterData) => filterData.Name == TargetKey);

        Assert.Single(filterData.Items); // No duplicate added
        Assert.Equal("John", filterData.Items[0].Value);

        Assert.True(requestFilters.ContainsKey(TargetKey));
        Assert.Equal("John", requestFilters[TargetKey][0]);
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
                    Items = [new FilterDataItem { Value = "John" }]
                }
            ]
        };
        Dictionary<string, string[]> requestFilters = [];
        CurrentFilterDetail filter = new()
        {
            FilterName = "Doe",
            FilterType = FilterType.Surname
        };

        // act
        _handler.Apply(filter, model, requestFilters);

        // assert
        FilterData filters =
            model.Filters.Single(filterData => filterData.Name == TargetKey);

        Assert.Equal(2, filters.Items.Count);
        Assert.Contains(filters.Items, i => i.Value == "John");
        Assert.Contains(filters.Items, i => i.Value == "Doe");

        Assert.True(requestFilters.ContainsKey(TargetKey));
        Assert.Equal(["John", "Doe"], requestFilters[TargetKey]);
    }

    [Fact]
    public void Apply_WithEmptyModelFilters_InitializesAndAddsEntry()
    {
        // arrange
        LearnerTextSearchViewModel model = new() { Filters = null };
        Dictionary<string, string[]> requestFilters = [];
        CurrentFilterDetail filter = new()
        {
            FilterName = "John",
            FilterType = FilterType.Forename
        };

        // act
        _handler.Apply(filter, model, requestFilters);

        // assert
        Assert.NotNull(model.Filters);
        Assert.Single(model.Filters);
        Assert.Equal("John", model.Filters[0].Items[0].Value);
        Assert.Equal("John", requestFilters[TargetKey][0]);
    }
}

