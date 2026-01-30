using Dfe.Data.Common.Infrastructure.CognitiveSearch.Filtering;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.Filtering.FilterExpressions.Formatters;

namespace DfE.GIAP.Core.Search.Infrastructure.Shared.SearchFilterExpressions;

/// <summary>
/// Concrete implementation of <see cref="BaseEqualityFilterExpression"/> that builds
/// a scalar equality filter expression for Azure Cognitive Search.
/// </summary>
/// <remarks>
/// This class targets fields that support direct equality comparison (e.g., scalar strings),
/// and does not use collection-aware syntax like <c>any(...)</c>.
/// </remarks>
public sealed class SearchByEqualityFilterExpression : BaseEqualityFilterExpression
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SearchByEqualityFilterExpression"/> class.
    /// </summary>
    /// <param name="formatter">
    /// Formatter used to construct the final OData expression and placeholder substitution.
    /// </param>
    public SearchByEqualityFilterExpression(IFilterExpressionFormatter formatter)
        : base(formatter) { }

    /// <summary>
    /// Constructs the final OData filter expression using scalar equality syntax.
    /// Example output: <c>subject eq 'Math,Science'</c>
    /// </summary>
    /// <param name="request">
    /// The validated filter request containing key and values.
    /// </param>
    /// <returns>
    /// A formatted filter expression string using <c>eq</c> for scalar fields.
    /// </returns>
    protected override string BuildExpression(SearchFilterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return FilterExpressionFormatter.CreateFormattedExpression(
            $"{request.FilterKey} eq " +
            $"'{FilterExpressionFormatter.CreateFilterCriteriaPlaceholders(request.FilterValues)}'",
            request.FilterValues);
    }
}
