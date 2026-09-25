using DfE.GIAP.Web.Features.Search.LegacyModels;
using DfE.GIAP.Web.Features.Search.Shared.Filters.Handlers;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Tests.Features.Search.Shared.Filters.Handlers;

public class DobFilterHandlerTests
{
    private readonly DobFilterHandler _handler = new();

    [Fact]
    public void Apply_WithDobError_DoesNotModifyRequestFilters()
    {
        // arrange
        LearnerTextSearchViewModel model = new()
        {
            FilterErrors = new FilterErrors { DobError = true }
        };

        Dictionary<string, string[]> filters = [];
        CurrentFilterDetail filterDetail = new()
        {
            FilterName = "15/3/2005",
            FilterType = FilterType.Dob
        };

        // act
        _handler.Apply(filterDetail, model, filters);

        // assert
        Assert.Empty(filters);
    }

    [Fact]
    public void Apply_WithFullDobComponents_AddsIsoFormattedDob()
    {
        // arrange
        LearnerTextSearchViewModel model = new()
        {
            FilterErrors = new FilterErrors(),
            SearchFilters = new SearchFilters
            {
                CustomFilterText = new CustomFilterText
                {
                    DobDay = 15,
                    DobMonth = 3,
                    DobYear = 2005
                }
            }
        };

        Dictionary<string, string[]> filters = [];
        CurrentFilterDetail filterDetail = new()
        {
            FilterName = "15/3/2005",
            FilterType = FilterType.Dob
        };

        // act
        _handler.Apply(filterDetail, model, filters);

        // assert
        Assert.True(filters.ContainsKey("DOB"));
        Assert.Equal("2005-03-15T00:00:00.000Z", filters["DOB"][0]);
    }

    [Fact]
    public void Apply_WithYearOnly_AddsDobYearFilter()
    {
        // arrange
        LearnerTextSearchViewModel model = new()
        {
            FilterErrors = new FilterErrors(),
            SearchFilters = new SearchFilters
            {
                CustomFilterText = new CustomFilterText
                {
                    DobYear = 2005
                }
            }
        };
        Dictionary<string, string[]> filters = [];
        CurrentFilterDetail filterDetail = new()
        {
            FilterName = "Dob",
            FilterType = FilterType.Dob
        };

        // act
        _handler.Apply(filterDetail, model, filters);

        // assert
        Assert.True(filters.ContainsKey("DOBYear"));
        Assert.Equal("2005", filters["DOBYear"][0]);
    }

    [Fact]
    public void Apply_WithMonthAndYear_AddsDobYearMonthFilter()
    {
        // arrange
        LearnerTextSearchViewModel model = new()
        {
            FilterErrors = new FilterErrors(),
            SearchFilters = new SearchFilters
            {
                CustomFilterText = new CustomFilterText
                {
                    DobMonth = 3,
                    DobYear = 2005
                }
            }
        };
        Dictionary<string, string[]> filters = new Dictionary<string, string[]>();
        CurrentFilterDetail filterDetail = new CurrentFilterDetail
        {
            FilterName = "Dob",
            FilterType = FilterType.Dob
        };

        // act
        _handler.Apply(filterDetail, model, filters);

        // assert
        Assert.True(filters.ContainsKey("DOBYearMonth"));
        Assert.Equal("2005-03", filters["DOBYearMonth"][0]);
    }

    [Fact]
    public void Apply_WithEmptyDobComponents_UsesFallbackFromFilterName()
    {
        // arrange
        LearnerTextSearchViewModel model = new()
        {
            FilterErrors = new FilterErrors(),
            SearchFilters = new SearchFilters
            {
                CustomFilterText = new CustomFilterText()
            }
        };
        Dictionary<string, string[]> filters = [];
        CurrentFilterDetail filterDetail = new()
        {
            FilterName = "15/3/2005",
            FilterType = FilterType.Dob
        };

        // act
        _handler.Apply(filterDetail, model, filters);

        // assert
        Assert.True(filters.ContainsKey("DOB"));
        Assert.Equal("2005-03-15T00:00:00.000Z", filters["DOB"][0]);
    }
}


