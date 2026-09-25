using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Web.Features.Search.LegacyModels.Learner;
using Moq;

namespace DfE.GIAP.Web.Tests.Features.Search.Shared.TestDoubles;

/// <summary>
/// Provides a reusable test double for <see cref="IMapper{FilterData, FilterRequest}"/> used in unit tests.
/// Supports both single and multi-expression setups for flexible mocking of filter mapping behavior.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class FilterRequestMapperTestDouble
{
    /// <summary>
    /// Shared mock instance for the mapper. Centralizes setup and reuse across tests.
    /// </summary>
    private static readonly Mock<IMapper<FilterData, FilterRequest>> _mock = new();

    /// <summary>
    /// Returns the raw mock instance for direct configuration or verification.
    /// </summary>
    public static Mock<IMapper<FilterData, FilterRequest>> Mock() => _mock;

    /// <summary>
    /// Configures the mock to return a specific <see cref="FilterRequest"/> when the input matches the given predicate.
    /// Useful for targeted, single-case setups in isolated tests.
    /// </summary>
    /// <param name="functionDelegate">Predicate expression to match incoming <see cref="FilterData"/>.</param>
    /// <param name="filterRequest">The expected result to return when the predicate matches.</param>
    /// <returns>The mock object with the setup applied.</returns>
    public static IMapper<FilterData, FilterRequest> MockForExpression(
        Expression<Func<FilterData, bool>> functionDelegate,
        FilterRequest filterRequest)
    {
        _mock
            .Setup(mapper => mapper.Map(It.Is(functionDelegate)))
            .Returns(filterRequest);

        return _mock.Object;
    }

    /// <summary>
    /// Configures the mock with multiple predicate-result pairs in a single call.
    /// Promotes modularity and reduces boilerplate in tests that validate multiple filter mappings.
    /// </summary>
    /// <param name="setups">
    /// A collection of tuples, each containing a predicate expression and the corresponding expected result.
    /// </param>
    /// <returns>The mock object with all setups applied.</returns>
    public static IMapper<FilterData, FilterRequest> MockForExpressions(
        IEnumerable<(Expression<Func<FilterData, bool>> predicate, FilterRequest result)> setups)
    {
        foreach ((Expression<Func<FilterData, bool>> predicate, FilterRequest result) in setups)
        {
            _mock
                .Setup(mapper => mapper.Map(It.Is(predicate)))
                .Returns(result);
        }

        return _mock.Object;
    }
}
