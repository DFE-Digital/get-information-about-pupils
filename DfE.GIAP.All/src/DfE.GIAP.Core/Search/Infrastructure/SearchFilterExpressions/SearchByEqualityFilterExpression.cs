using Dfe.Data.Common.Infrastructure.CognitiveSearch.Filtering.FilterExpressions.Formatters;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.Filtering.FilterExpressions;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.Filtering;

namespace DfE.GIAP.Core.Search.Infrastructure.SearchFilterExpressions;

/// <summary>
/// Represents a filter expression that performs equality-based matching
/// against a specified field in Azure Cognitive Search.
/// </summary>
/// <remarks>
/// This implementation supports collection-valued fields using OData 'any' and 'eq' syntax.
/// Boolean values are not supported as filter criteria.
/// </remarks>
public sealed class SearchByEqualityFilterExpression : ISearchFilterExpression
{
    private readonly IFilterExpressionFormatter _filterExpressionFormatter;

    /// <summary>
    /// Default logical operator used to join multiple filter values.
    /// For collection-valued fields, this is typically <c>or</c>.
    /// </summary>
    private const string DefaultFilterValuesDelimiter = " or ";

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchByEqualityFilterExpression"/> class.
    /// </summary>
    /// <param name="filterExpressionFormatter">
    /// The formatter responsible for constructing OData-compliant filter expressions.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="filterExpressionFormatter"/> is <c>null</c>.
    /// </exception>
    public SearchByEqualityFilterExpression(IFilterExpressionFormatter filterExpressionFormatter)
    {
        _filterExpressionFormatter =
            filterExpressionFormatter ??
            throw new ArgumentNullException(nameof(filterExpressionFormatter));
    }

    /// <summary>
    /// Generates an equality-based filter expression for Azure Cognitive Search.
    /// </summary>
    /// <param name="searchFilterRequest">
    /// The request containing the filter key, values, and optional delimiter.
    /// </param>
    /// <returns>
    /// A formatted OData filter expression string using equality comparison.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="searchFilterRequest"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if any value in <c>FilterValues</c> is a boolean, which is unsupported.
    /// </exception>
    /// <example>
    /// For a filter key "subject" and values ["Math", "Science"], the output might be:
    /// <c>subject eq 'Math,Science'</c>
    /// </example>
    public string GetFilterExpression(SearchFilterRequest searchFilterRequest)
    {
        ArgumentNullException.ThrowIfNull(searchFilterRequest);

        // Validate that the filter values are compatible with string-based filtering.
        searchFilterRequest.FilterValues.ToList()
            .ForEach(filterValue =>
            {
                if (filterValue is bool)
                {
                    throw new ArgumentException(
                        "Invalid boolean type argument for filter key", searchFilterRequest.FilterKey);
                }
            });

        // Apply default logical delimiter if none is provided.
        if (string.IsNullOrWhiteSpace(searchFilterRequest.FilterValuesDelimiter))
        {
            searchFilterRequest.SetFilterValuesDelimiter(DefaultFilterValuesDelimiter);
        }

        // Configure the formatter with the chosen logical operator (e.g., "or").
        _filterExpressionFormatter.SetExpressionParamsSeparator(searchFilterRequest.FilterValuesDelimiter);

        // Construct the final OData expression using 'any' and 'eq' for collection-valued fields.
        return _filterExpressionFormatter
                .CreateFormattedExpression(
                    $"{searchFilterRequest.FilterKey} eq " +
                    $"'{_filterExpressionFormatter.CreateFilterCriteriaPlaceholders(searchFilterRequest.FilterValues)}'",
                    searchFilterRequest.FilterValues);
    }
}
