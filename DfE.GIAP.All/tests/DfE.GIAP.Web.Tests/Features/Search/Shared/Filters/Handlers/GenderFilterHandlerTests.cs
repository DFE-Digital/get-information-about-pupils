using DfE.GIAP.Web.Features.Search.LegacyModels;
using DfE.GIAP.Web.Features.Search.LegacyModels.Learner;
using DfE.GIAP.Web.Features.Search.Shared.Filters.Handlers;
using DfE.GIAP.Web.ViewModels.Search;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.Search.Shared.Filters.Handlers;

public class GenderFilterHandlerTests
{
    private const string TargetKey = "Sex";
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
            FilterType = FilterType.Sex
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
            FilterType = FilterType.Sex
        };

        // act
        _handler.Apply(filter, model, requestFilters);

        // assert
        FilterData filterData =
            model.Filters.Single((filterData) => filterData.Name == TargetKey);

        Assert.Single(filterData.Items); // No duplicate added
        Assert.Equal("F", filterData.Items[0].Value);

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
            FilterName = "M",
            FilterType = FilterType.Sex
        };

        // act
        _handler.Apply(filter, model, requestFilters);

        // assert
        FilterData filters =
            model.Filters.Single(filterData => filterData.Name == TargetKey);

        Assert.Equal(2, filters.Items.Count);
        Assert.Contains(filters.Items, i => i.Value == "F");
        Assert.Contains(filters.Items, i => i.Value == "M");

        Assert.True(requestFilters.ContainsKey(TargetKey));
        Assert.Equal(["F", "M"], requestFilters[TargetKey]);
    }

    [Fact]
    public void Apply_WithEmptyModelFilters_InitializesAndAddsEntry()
    {
        // arrange
        LearnerTextSearchViewModel model = new() { Filters = null };
        Dictionary<string, string[]> requestFilters = [];
        CurrentFilterDetail filter = new()
        {
            FilterName = "F",
            FilterType = FilterType.Sex
        };

        // act
        _handler.Apply(filter, model, requestFilters);

        // assert
        Assert.NotNull(model.Filters);
        Assert.Single(model.Filters);
        Assert.Equal("F", model.Filters[0].Items[0].Value);
        Assert.Equal("F", requestFilters[TargetKey][0]);
    }
}

