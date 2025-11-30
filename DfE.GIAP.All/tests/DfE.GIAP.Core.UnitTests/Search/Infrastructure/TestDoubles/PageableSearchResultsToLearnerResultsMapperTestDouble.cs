using System.Linq.Expressions;
using Azure;
using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Search.Application.Models.Learner;
using DfE.GIAP.Core.Search.Infrastructure.DataTransferObjects;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;

/// <summary>
/// Provides test doubles for mapping Azure pageable search results into domain-specific learner aggregates.
/// Enables isolated testing of downstream logic without invoking real mapping implementations.
/// </summary>
internal static class PageableSearchResultsToLearnerResultsMapperTestDouble
{
    /// <summary>
    /// Returns a default mock of the IMapper interface for transforming Azure search results into Learners.
    /// Used to simulate mapping behavior in unit tests without relying on actual transformation logic.
    /// </summary>
    public static IMapper<Pageable<SearchResult<LearnerDataTransferObject>>, Learners> DefaultMock() =>
        Mock.Of<IMapper<Pageable<SearchResult<LearnerDataTransferObject>>, Learners>>();

    /// <summary>
    /// Provides an expression tree representing a call to the Map method on the IMapper interface.
    /// Used to configure mock behavior symbolically and support traceable test setups.
    /// </summary>
    public static Expression<Func<IMapper<Pageable<SearchResult<LearnerDataTransferObject>>, Learners>, Learners>> MapFrom() =>
        mapper => mapper.Map(It.IsAny<Pageable<SearchResult<LearnerDataTransferObject>>>());

    /// <summary>
    /// Returns a mock mapper configured to throw an ArgumentException when Map is invoked.
    /// Useful for testing error-handling paths and validating robustness of consuming logic.
    /// </summary>
    public static IMapper<Pageable<SearchResult<LearnerDataTransferObject>>, Learners> MockMapperThrowingArgumentException()
    {
        // Create a mock instance of the IMapper interface
        Mock<IMapper<Pageable<SearchResult<LearnerDataTransferObject>>, Learners>> mapperMock = new();

        // Configure the mock to throw an exception when Map is called
        mapperMock.Setup(MapFrom()).Throws(new ArgumentException());

        // Return the mock object for injection into test scenarios
        return mapperMock.Object;
    }
}
