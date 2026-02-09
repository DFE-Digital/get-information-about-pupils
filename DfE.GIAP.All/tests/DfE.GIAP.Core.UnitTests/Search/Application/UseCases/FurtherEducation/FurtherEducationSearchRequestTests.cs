using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Sort;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.SearchByName;
using DfE.GIAP.Core.UnitTests.Search.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles;
using FluentAssertions;

namespace DfE.GIAP.Core.UnitTests.Search.Application.UseCases.FurtherEducation;

public sealed class FurtherEducationSearchRequestTests
{
    [Fact]
    public void Constructor_WithFilterParam_PopulatesFilterRequests()
    {
        // arrange
        SearchCriteria searchCriteria = SearchCriteriaTestDouble.Stub();
        List<FilterRequest> filterRequests = [FilterRequestTestDouble.Fake()];
        SortOrder sortOrder = SortOrderTestDouble.Stub();

        // act
        FurtherEducationSearchByNameRequest request = new()
        {
            SearchKeywords = "searchKeyword",
            FilterRequests = filterRequests,
            SearchCriteria = searchCriteria,
            SortOrder = sortOrder
        };

        // assert
        request.FilterRequests.Should().NotBeNull();
        request.SearchCriteria.Should().NotBeNull();
        request.SortOrder.Should().NotBeNull();

        foreach (FilterRequest item in filterRequests)
        {
            FilterRequest matchingRequest =
                request.FilterRequests!.First(filterRequest =>
                filterRequest.FilterName == item.FilterName);

            matchingRequest.FilterValues.Should().BeEquivalentTo(item.FilterValues);
        }
    }

    [Fact]
    public void Constructor_WithNoFilterParam_HasFilterRequestsNull()
    {
        // arrange
        SortOrder sortOrder = SortOrderTestDouble.Stub();
        SearchCriteria searchCriteria = SearchCriteriaTestDouble.Stub();

        // act
        FurtherEducationSearchByNameRequest request = new()
        {
            SearchKeywords = "searchKeyword",
            SearchCriteria = searchCriteria,
            SortOrder = sortOrder
        };

        // assert
        request.SortOrder.Should().NotBeNull();
        request.SearchCriteria.Should().NotBeNull();
        request.FilterRequests.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithSetOffsetValue_AssignsCorrectPropertyValue()
    {
        //arrange
        SortOrder sortOrder = SortOrderTestDouble.Stub();
        SearchCriteria searchCriteria = SearchCriteriaTestDouble.Stub();
        const int Offset = 10;

        // act
        FurtherEducationSearchByNameRequest request = new()
        {
            SearchKeywords = "searchKeyword",
            SortOrder = sortOrder,
            SearchCriteria = searchCriteria,
            Offset = Offset
        };

        // assert
        request.SortOrder.Should().NotBeNull();
        request.SearchCriteria.Should().NotBeNull();
        request.Offset.Should().Be(Offset);
    }

    [Fact]
    public void Constructor_WithDefaultOffsetValue_AssignsDefaultPropertyValue()
    {
        // act
        SortOrder sortOrder = SortOrderTestDouble.Stub();
        SearchCriteria searchCriteria = SearchCriteriaTestDouble.Stub();

        // Act
        FurtherEducationSearchByNameRequest request = new()
        {
            SearchKeywords = "searchKeyword",
            SearchCriteria = searchCriteria,
            SortOrder = sortOrder
        };

        // assert
        request.SortOrder.Should().NotBeNull();
        request.SearchCriteria.Should().NotBeNull();
        request.Offset.Should().Be(0);  //value of zero ensures no records are skipped
    }
}
