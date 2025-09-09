using Dfe.Data.Common.Infrastructure.CognitiveSearch.Filtering;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.Filtering.FilterExpressions;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.Filtering.FilterExpressions.Formatters;

namespace DfE.GIAP.Core.Search.Infrastructure.SearchFilterExpressions;

/// <summary>
/// Creates an OData filter expression for a collection-valued facet field in Azure Cognitive Search.
/// This expression uses the <c>any(...)</c> operator to match one or more values within a multi-valued field,
/// such as <c>Collection(Edm.String)</c>. It does not use <c>search.in</c>, which is only valid for scalar fields.
/// </summary>
public sealed class SearchCollectionValuedFilterExpression : ISearchFilterExpression
{
    private readonly IFilterExpressionFormatter _filterExpressionFormatter;

    /// <summary>
    /// Default logical operator used to join multiple filter values.
    /// For collection-valued fields, this is typically <c>or</c>.
    /// </summary>
    private const string DefaultFilterValuesDelimiter = " or ";

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchCollectionValuedFilterExpression"/> class.
    /// Uses a formatter to construct the final OData expression string.
    /// </summary>
    /// <param name="filterExpressionFormatter">
    /// Formatter used to generate placeholder strings and delimiters for filter values.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="filterExpressionFormatter"/> is null.
    /// </exception>
    public SearchCollectionValuedFilterExpression(IFilterExpressionFormatter filterExpressionFormatter)
    {
        _filterExpressionFormatter =
            filterExpressionFormatter ??
            throw new ArgumentNullException(nameof(filterExpressionFormatter));
    }

    /// <summary>
    /// Builds an OData filter expression for a collection-valued facet field.
    /// Uses the <c>any</c> operator to match values within the field.
    /// Validates that all filter values are string-compatible (e.g., not boolean).
    /// </summary>
    /// <param name="searchFilterRequest">
    /// Contains the filter key (field name), values to match, and optional delimiter override.
    /// </param>
    /// <returns>
    /// A formatted OData filter string using <c>any</c> and <c>eq</c> expressions.
    /// Example: <c>tags/any(s: s eq 'Math' or s eq 'Science')</c>
    /// </returns>
    public string GetFilterExpression(SearchFilterRequest searchFilterRequest)
    {
        ArgumentNullException.ThrowIfNull(searchFilterRequest);

        // Validate that all filter values are compatible with string-based filtering.
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
                $"{searchFilterRequest.FilterKey}/any(s: s eq " +
                $"'{_filterExpressionFormatter.CreateFilterCriteriaPlaceholders(searchFilterRequest.FilterValues)}')",
                searchFilterRequest.FilterValues);
    }
}
