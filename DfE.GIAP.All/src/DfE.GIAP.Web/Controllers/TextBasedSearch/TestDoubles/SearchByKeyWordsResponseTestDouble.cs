using DfE.GIAP.Core.Search.Application.Models.Learner;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.UseCases.Response;

namespace DfE.GIAP.Web.Controllers.TextBasedSearch.TestDoubles;

/// <summary>
/// Provides a stubbed <see cref="SearchByKeyWordsResponse"/> instance for unit testing.
/// Supports controlled injection of learner results, facet data, and status metadata.
/// </summary>
public static class SearchByKeyWordsResponseTestDouble
{
    /// <summary>
    /// Creates a stubbed response with one learner and one facet result.
    /// </summary>
    public static SearchByKeyWordsResponse CreateSuccessResponse()
    {
        Learners learners =
            new(
                [new(
                    new LearnerIdentifier("1234567890"),
                    new LearnerName("Alice", "Smith"),
                    new LearnerCharacteristics(
                        new DateTime(2005, 6, 1),
                        LearnerCharacteristics.Gender.Female)
                    )
                ]
            );

        SearchFacets facets = new(
            [
                new("Region",
                [
                    new FacetResult("North", 10)
                ])
            ]
        );

        return new SearchByKeyWordsResponse(SearchResponseStatus.Success, learners.Count)
        {
            LearnerSearchResults = learners,
            FacetedResults = facets
        };
    }

    /// <summary>
    /// Creates a stubbed response with no results and a NoResultsFound status.
    /// </summary>
    public static SearchByKeyWordsResponse CreateEmptyResponse()
    {
        return new SearchByKeyWordsResponse(SearchResponseStatus.NoResultsFound, 0)
        {
            LearnerSearchResults = Learners.CreateEmpty(),
            FacetedResults = new SearchFacets()
        };
    }

    /// <summary>
    /// Creates a stubbed response with a custom status and result count.
    /// </summary>
    public static SearchByKeyWordsResponse CreateCustomResponse(
        SearchResponseStatus status,
        int totalCount,
        Learners learners = null,
        SearchFacets facets = null) =>
            new(status, totalCount)
            {
                LearnerSearchResults = learners,
                FacetedResults = facets
            };
}

