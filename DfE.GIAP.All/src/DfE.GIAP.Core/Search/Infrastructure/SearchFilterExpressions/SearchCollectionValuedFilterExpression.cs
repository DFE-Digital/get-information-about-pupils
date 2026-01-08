using Dfe.Data.Common.Infrastructure.CognitiveSearch.Filtering;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.Filtering.FilterExpressions.Formatters;

namespace DfE.GIAP.Core.Search.Infrastructure.SearchFilterExpressions;

/// <summary>
/// Concrete implementation of <see cref="BaseEqualityFilterExpression"/> that builds
/// a collection-valued filter expression using the OData <c>any(...)</c> operator.
/// </summary>
/// <remarks>
/// This class targets fields of type <c>Collection(Edm.String)</c> in Azure Cognitive Search,
/// enabling multi-value matching via <c>any</c> and <c>eq</c> syntax.
/// </remarks>
public sealed class SearchCollectionValuedFilterExpression : BaseEqualityFilterExpression
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SearchCollectionValuedFilterExpression"/> class.
    /// </summary>
    /// <param name="formatter">
    /// Formatter used to construct the final OData expression and placeholder substitution.
    /// </param>
    public SearchCollectionValuedFilterExpression(IFilterExpressionFormatter formatter)
        : base(formatter) { }

    /// <summary>
    /// Constructs the final OData filter expression using collection-aware syntax.
    /// Example output: <c>tags/any(s: s eq 'Math' or s eq 'Science')</c>
    /// </summary>
    /// <param name="request">
    /// The validated filter request containing key and values.
    /// </param>
    /// <returns>
    /// A formatted filter expression string using <c>any</c> and <c>eq</c> for multi-valued fields.
    /// </returns>
    protected override string BuildExpression(SearchFilterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return FilterExpressionFormatter.CreateFormattedExpression(
            $"{request.FilterKey}/any(s: s eq " +
            $"'{FilterExpressionFormatter.CreateFilterCriteriaPlaceholders(request.FilterValues)}')",
            request.FilterValues);
    }
}
