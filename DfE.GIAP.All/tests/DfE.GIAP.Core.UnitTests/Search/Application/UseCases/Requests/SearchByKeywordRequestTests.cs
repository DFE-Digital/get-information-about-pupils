using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Sort;
using DfE.GIAP.Core.Search.Application.UseCases.Request;
using DfE.GIAP.Core.UnitTests.Search.Application.UseCases.TestDoubles;
using FluentAssertions;

namespace DfE.GIAP.Core.UnitTests.Search.Application.UseCases.Requests;

public class SearchByKeywordRequestTests
{
    [Fact]
    public void Constructor_WithFilterParam_PopulatesFilterRequests()
    {
        // arrange
        List<FilterRequest> filterRequests = [FilterRequestTestDouble.Fake()];
        SortOrder sortOrder = SortOrderTestDouble.Stub();

        // act
        SearchRequest request = new(
            searchIndexKey: "stubIndexKey",
            searchKeywords: "searchKeyword",
            filterRequests: filterRequests,
            sortOrder: sortOrder);

        // assert
        request.FilterRequests.Should().NotBeNull();
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

        // act
        SearchRequest request = new(
            searchIndexKey: "stubIndexKey",
            searchKeywords: "searchKeyword",
            sortOrder: sortOrder);

        // assert
        request.SortOrder.Should().NotBeNull();
        request.FilterRequests.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithSetOffsetValue_AssignsCorrectPropertyValue()
    {
        //arrange
        SortOrder sortOrder = SortOrderTestDouble.Stub();
        const int Offset = 10;

        // act
        SearchRequest request = new(
            searchIndexKey: "stubIndexKey",
            searchKeywords: "searchKeyword",
            sortOrder: sortOrder,
            offset: Offset);

        // assert
        request.SortOrder.Should().NotBeNull();
        request.Offset.Should().Be(Offset);
    }

    [Fact]
    public void Constructor_WithDefaultOffsetValue_AssignsDefaultPropertyValue()
    {
        // act
        SortOrder sortOrder = SortOrderTestDouble.Stub();
        SearchRequest request = new(
            searchIndexKey: "stubIndexKey",
            searchKeywords: "searchKeyword",
            sortOrder: sortOrder);

        // assert
        request.SortOrder.Should().NotBeNull();
        request.Offset.Should().Be(0);  //value of zero ensures no records are skipped
    }
}
