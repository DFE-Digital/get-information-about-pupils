using Dfe.Data.Common.Infrastructure.CognitiveSearch.Filtering;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.Filtering.FilterExpressions.Formatters;
using DfE.GIAP.Core.Search.Infrastructure.SearchFilterExpressions;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.SearchFilterExpressions;

public sealed class SearchByEqualityFilterExpressionTests
{
    [Fact]
    public void GetFilterExpression_ValidFilterValueSpecified_ReturnsFormattedEqualityExpression()
    {
        // arrange
        SearchByEqualityFilterExpression filterExpression =
            new(new DefaultFilterExpressionFormatter());

        SearchFilterRequest request = new("filter", ["value 1"]);

        const string expected = "filter eq 'value 1'";

        // act
        string result = filterExpression.GetFilterExpression(request);

        // assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetFilterExpression_BoolFilterValuesAndDelimiterSpecified_ThrowsArgumentException()
    {
        // arrange
        SearchByEqualityFilterExpression filterExpression =
            new(new DefaultFilterExpressionFormatter());

        SearchFilterRequest request = new("filter", [true]);

        // act, assert
        Assert.Throws<ArgumentException>(() =>
            filterExpression.GetFilterExpression(request));
    }

    [Fact]
    public void Ctor_NullFilterExpressionFormatter_ThrowsArgumentException()
    {
        // arrange/act.
        Action failedCtorAction = static () =>
            new SearchByEqualityFilterExpression(formatter: null!);

        //assert
        ArgumentException exception =
            Assert.Throws<ArgumentNullException>(failedCtorAction);
        Assert.Equal("Value cannot be null. (Parameter 'filterExpressionFormatter')", exception.Message);
    }
}
