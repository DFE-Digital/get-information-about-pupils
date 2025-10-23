using Dfe.Data.Common.Infrastructure.CognitiveSearch.Filtering;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.Filtering.FilterExpressions.Formatters;
using DfE.GIAP.Core.Search.Infrastructure.SearchFilterExpressions;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.SearchFilterExpressions;

public sealed class SearchCollectionValuedFilterExpressionTests
{
    [Fact]
    public void GetFilterExpression_ValidFilterValueSpecified_ReturnsFormattedEqualityExpression()
    {
        // arrange
        SearchCollectionValuedFilterExpression filterExpression =
            new(new DefaultFilterExpressionFormatter());

        SearchFilterRequest request = new("filter", ["value 1"]);

        const string expected = "filter/any(s: s eq 'value 1')";

        // act
        string result = filterExpression.GetFilterExpression(request);

        // assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetFilterExpression_MultipleValidFilterValueSpecified_ReturnsFormattedEqualityExpression()
    {
        // arrange
        SearchCollectionValuedFilterExpression filterExpression =
            new(new DefaultFilterExpressionFormatter());

        SearchFilterRequest request = new("filter", ["value 1", "value 2"]);

        const string expected = "filter/any(s: s eq 'value 1 or value 2')";

        // act
        string result = filterExpression.GetFilterExpression(request);

        // assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetFilterExpression_BoolFilterValuesAndDelimiterSpecified_ThrowsArgumentException()
    {
        // arrange
        SearchCollectionValuedFilterExpression filterExpression =
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
            new SearchCollectionValuedFilterExpression(formatter: null!);

        //assert
        ArgumentException exception =
            Assert.Throws<ArgumentNullException>(failedCtorAction);
        Assert.Equal("Value cannot be null. (Parameter 'filterExpressionFormatter')", exception.Message);
    }
}
