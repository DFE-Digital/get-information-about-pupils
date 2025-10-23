using System.Linq.Expressions;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Web.Controllers.TextBasedSearch.Mappers;
using DfE.GIAP.Web.Tests.Controllers.Search.TextBasedSearch.Mappers.TestDoubles;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.TextBasedSearch.Mappers;

public sealed class FiltersRequestMapperTests
{
    [Fact]
    public void Constructor_WithNullMapper_ThrowsArgumentNullException()
    {
        // act & assert
        Assert.Throws<ArgumentNullException>(() => new FiltersRequestMapper(null));
    }

    [Fact]
    public void Map_WithEmptyDictionary_ReturnsEmptyList()
    {
        // arrange
        Dictionary<string, string[]> input = [];
        Mock<IMapper<FilterData, FilterRequest>> mockFilterRequestMapper =
            FilterRequestMapperTestDouble.Mock();

        FiltersRequestMapper mapper = new(mockFilterRequestMapper.Object);

        // act
        IList<FilterRequest> result = mapper.Map(input);

        // assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void Map_WithSingleFilter_DelegatesToInjectedMapper()
    {
        // arrange
        Dictionary<string, string[]> input = new()
        {
            { "Status", ["Active", "Inactive"] }
        };

        FilterRequest expectedFilterRequest =
            new("Status", ["Active", "Inactive"]);

        IMapper<FilterData, FilterRequest> mockFilterRequestMapper =
            FilterRequestMapperTestDouble.MockForExpression(functionDelegate =>
                functionDelegate.Name == "Status" &&
                functionDelegate.Items.Select(filterDataItem =>
                    filterDataItem.Value).SequenceEqual(new[] { "Active", "Inactive" }),
                expectedFilterRequest);

        FiltersRequestMapper mapper = new(mockFilterRequestMapper);

        // act
        IList<FilterRequest> result = mapper.Map(input);

        // assert
        Assert.Single(result);
        Assert.Equal(expectedFilterRequest, result[0]);
    }

    [Fact]
    public void Map_WithMultipleFilters_DelegatesEachToInjectedMapper()
    {
        // arrange
        Dictionary<string, string[]> input = new()
        {
            { "Region", ["North"] },
            { "Status", ["Active", "Inactive"] }
        };

        List<FilterRequest> expectedRequests =
        [
            new FilterRequest("Region", ["North"]),
            new FilterRequest("Status", ["Active", "Inactive"])
        ];

        List<(Expression<Func<FilterData, bool>>, FilterRequest)> setups =
        [
            (functionDelegate =>
                functionDelegate.Name == "Status",
                new FilterRequest("Status", ["Active"])),
            (functionDelegate =>
                functionDelegate.Name == "Region",
                new FilterRequest("Region", ["North"]))
        ];

        IMapper<FilterData, FilterRequest> mockFilterRequestMapper =
            FilterRequestMapperTestDouble.MockForExpressions(setups);

        FiltersRequestMapper mapper = new(mockFilterRequestMapper);

        // act
        IList<FilterRequest> result = mapper.Map(input);

        // assert
        Assert.Equal(2, result.Count);
        Assert.Matches(expectedRequests[0].FilterName, result[0].FilterName);
        Assert.Matches(expectedRequests[0].FilterValues[0].ToString()!, result[0].FilterValues[0].ToString());
        Assert.Equivalent(expectedRequests[1].FilterName, result[1].FilterName);
        Assert.Matches(expectedRequests[1].FilterValues[0].ToString()!, result[1].FilterValues[0].ToString());
    }
}
