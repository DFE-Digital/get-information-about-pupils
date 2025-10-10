using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.Filtering;
using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Infrastructure.Builders;
using DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.Builders;

public sealed class SearchOptionsBuilderTests
{
    [Fact]
    public void Build_WithSize_SearchOptionsWithCorrectSize()
    {
        // arrange
        ISearchFilterExpressionsBuilder mockSearchFilterExpressionsBuilder =
            FilterExpressionTestDouble.Mock();

        SearchOptionsBuilder searchOptionsBuilder =
            new(mockSearchFilterExpressionsBuilder);

        // act
        SearchOptions searchOptions =
            searchOptionsBuilder.WithSize(size: 100).Build();

        // assert
        Assert.NotNull(searchOptions);
        Assert.Equal(100, searchOptions.Size);
    }

    [Fact]
    public void Build_WithOffset_SearchOptionsWithCorrectOffset()
    {
        // arrange
        ISearchFilterExpressionsBuilder mockSearchFilterExpressionsBuilder =
            FilterExpressionTestDouble.Mock();

        SearchOptionsBuilder searchOptionsBuilder =
            new(mockSearchFilterExpressionsBuilder);

        // act
        SearchOptions searchOptions =
            searchOptionsBuilder.WithOffset(offset: 39).Build();

        // assert
        Assert.NotNull(searchOptions);
        Assert.Equal(39, searchOptions.Skip);
    }

    [Fact]
    public void Build_WithSearchMode_SearchOptionsWithCorrectSearchMode()
    {
        // arrange
        ISearchFilterExpressionsBuilder mockSearchFilterExpressionsBuilder =
            FilterExpressionTestDouble.Mock();

        SearchOptionsBuilder searchOptionsBuilder =
            new(mockSearchFilterExpressionsBuilder);

        // act
        SearchOptions searchOptions =
            searchOptionsBuilder.WithSearchMode(searchMode: SearchMode.Any).Build();

        // assert
        Assert.NotNull(searchOptions);
        Assert.Equal(SearchMode.Any, searchOptions.SearchMode);
    }

    [Fact]
    public void Build_WithIncludeTotalCount_SearchOptionsWithIncludeTotalCount()
    {
        // arrange
        ISearchFilterExpressionsBuilder mockSearchFilterExpressionsBuilder =
            FilterExpressionTestDouble.Mock();

        SearchOptionsBuilder searchOptionsBuilder =
            new(mockSearchFilterExpressionsBuilder);

        // act
        SearchOptions searchOptions =
            searchOptionsBuilder.WithIncludeTotalCount(includeTotalCount: true).Build();

        // assert
        Assert.NotNull(searchOptions);
        Assert.True(searchOptions.IncludeTotalCount);
    }

    [Fact]
    public void Build_WithSearchFields_SearchOptionsWithWithSearchFields()
    {
        // arrange
        ISearchFilterExpressionsBuilder mockSearchFilterExpressionsBuilder =
            FilterExpressionTestDouble.Mock();

        SearchOptionsBuilder searchOptionsBuilder =
            new(mockSearchFilterExpressionsBuilder);

        // act
        List<string> searchFields = ["FIELD_1", "FIELD_2", "FIELD_3"];

        SearchOptions searchOptions = searchOptionsBuilder.WithSearchFields(searchFields).Build();

        // assert
        Assert.NotNull(searchOptions);
        Assert.Equal(searchFields, searchOptions.SearchFields);
    }

    [Fact]
    public void Build_WithFacets_SearchOptionsWithWithFacets()
    {
        // arrange
        ISearchFilterExpressionsBuilder mockSearchFilterExpressionsBuilder =
            FilterExpressionTestDouble.Mock();

        SearchOptionsBuilder searchOptionsBuilder =
            new(mockSearchFilterExpressionsBuilder);

        // act
        List<string> searchFacets = ["FACET_1", "FACET_2", "FACET_3"];

        SearchOptions searchOptions = searchOptionsBuilder.WithFacets(searchFacets).Build();

        // assert
        Assert.NotNull(searchOptions);
        Assert.Equal(searchFacets, searchOptions.Facets);
    }

    [Fact]
    public void Build_WithFilters_CallsFilterBuilder_WithComposedFilterRequests()
    {
        // arrange
        List<FilterRequest> serviceAdapterInputFilterRequest =
            [
                FilterRequestTestDouble.Fake(),
                FilterRequestTestDouble.Fake()
            ];

        Mock<ISearchFilterExpressionsBuilder> mockSearchFilterExpressionsBuilder = new();
        List<SearchFilterRequest> requestMadeToFilterExpressionBuilder = [];

        mockSearchFilterExpressionsBuilder
            .Setup(builder => builder.BuildSearchFilterExpressions(It.IsAny<IEnumerable<SearchFilterRequest>>()))
            .Callback<IEnumerable<SearchFilterRequest>>((request) =>
                    requestMadeToFilterExpressionBuilder = request.ToList())
            .Returns("some filter string");

        SearchOptionsBuilder searchOptionsBuilder =
            new(mockSearchFilterExpressionsBuilder.Object);

        // act
        _ = searchOptionsBuilder.WithFilters(serviceAdapterInputFilterRequest).Build();

        // assert
        foreach (FilterRequest filterRequest in serviceAdapterInputFilterRequest)
        {
            SearchFilterRequest matchingFilterRequest =
                requestMadeToFilterExpressionBuilder
                    .First(request =>
                        request.FilterKey == filterRequest.FilterName);

            Assert.NotNull(matchingFilterRequest);
            Assert.Equal(filterRequest.FilterValues, matchingFilterRequest.FilterValues);
        }
    }

    [Fact]
    public void Build_WithFiltersNullSearchFilterExpressionsBuilder_ByPassesFilterBuilderCall()
    {
        // arrange
        List<FilterRequest> serviceAdapterInputFilterRequest =
        [
            FilterRequestTestDouble.Fake(),
            FilterRequestTestDouble.Fake()
        ];

        SearchOptionsBuilder searchOptionsBuilder =
            new(searchFilterExpressionsBuilder: null);

        // act
        SearchOptions searchOptions =
            searchOptionsBuilder.WithFilters(serviceAdapterInputFilterRequest).Build();

        // assert
        Assert.Null(searchOptions.Filter);
    }
}
