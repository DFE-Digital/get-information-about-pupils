using Dfe.Data.Common.Infrastructure.CognitiveSearch.Filtering;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.Filtering.FilterExpressions;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.Filtering.FilterExpressions.Formatters;

namespace DfE.GIAP.Core.Search.Infrastructure.SearchFilterExpressions;

/// <summary>
/// Abstract base class for constructing equality-based filter expressions
/// in Azure Cognitive Search. Encapsulates shared validation and formatting logic
/// for filter requests, allowing subclasses to define specific expression syntax.
/// </summary>
public abstract class BaseEqualityFilterExpression : ISearchFilterExpression
{
    /// <summary>
    /// Formatter responsible for generating OData-compliant filter expressions
    /// and placeholder substitution logic.
    /// </summary>
    protected readonly IFilterExpressionFormatter FilterExpressionFormatter;

    /// <summary>
    /// Default logical operator used to join multiple filter values
    /// when no delimiter is explicitly provided in the request.
    /// </summary>
    private const string DefaultFilterValuesDelimiter = " or ";

    /// <summary>
    /// Initializes the base filter expression with a formatter dependency.
    /// </summary>
    /// <param name="filterExpressionFormatter">
    /// Formatter used to construct the final filter expression.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the formatter is null, ensuring safe instantiation.
    /// </exception>
    protected BaseEqualityFilterExpression(IFilterExpressionFormatter filterExpressionFormatter)
    {
        FilterExpressionFormatter = filterExpressionFormatter
            ?? throw new ArgumentNullException(nameof(filterExpressionFormatter));
    }

    /// <summary>
    /// Generates a filter expression string based on the provided request.
    /// Applies validation, default delimiter logic, and delegates expression
    /// construction to the subclass via <see cref="BuildExpression"/>.
    /// </summary>
    /// <param name="searchFilterRequest">
    /// Contains the filter key, values, and optional delimiter override.
    /// </param>
    /// <returns>
    /// A formatted OData filter expression string.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the request is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if any filter value is a boolean, which is unsupported.
    /// </exception>
    public string GetFilterExpression(SearchFilterRequest searchFilterRequest)
    {
        // Ensure the request object is not null.
        ArgumentNullException.ThrowIfNull(searchFilterRequest);

        // Validate that all filter values are compatible with string-based filtering.
        ValidateFilterValues(searchFilterRequest);

        // Apply default logical delimiter if none is provided.
        if (string.IsNullOrWhiteSpace(searchFilterRequest.FilterValuesDelimiter))
        {
            searchFilterRequest.SetFilterValuesDelimiter(DefaultFilterValuesDelimiter);
        }

        // Configure the formatter with the chosen logical operator (e.g., "or").
        FilterExpressionFormatter.SetExpressionParamsSeparator(searchFilterRequest.FilterValuesDelimiter);

        // Delegate final expression construction to subclass implementation.
        return BuildExpression(searchFilterRequest);
    }

    /// <summary>
    /// Validates that all filter values are non-boolean.
    /// Boolean values are not supported in equality-based filtering.
    /// </summary>
    /// <param name="request">
    /// The filter request containing values to validate.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if any value is a boolean.
    /// </exception>
    private void ValidateFilterValues(SearchFilterRequest request)
    {
        request.FilterValues.ToList().ForEach(value =>
        {
            if (value is bool)
            {
                throw new ArgumentException(
                    "Invalid boolean type argument for filter key", request.FilterKey);
            }
        });
    }

    /// <summary>
    /// Abstract method for constructing the final filter expression string.
    /// Subclasses must implement this to define specific syntax (e.g., scalar vs. collection-valued).
    /// </summary>
    /// <param name="request">
    /// The validated filter request.
    /// </param>
    /// <returns>
    /// A formatted OData filter expression string.
    /// </returns>
    protected abstract string BuildExpression(SearchFilterRequest request);
}
