using Azure;
using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Search.Application.Models.Learner;
using DfE.GIAP.Core.Search.Infrastructure.DataTransferObjects;

namespace DfE.GIAP.Core.Search.Infrastructure.Mappers;

/// <summary>
/// Maps a pageable collection of Azure Search results containing <see cref="LearnerDataTransferObject"/>
/// into a domain model <see cref="Learners"/> collection.
/// </summary>
public sealed class PageableSearchResultsToLearnerResultsMapper :
    IMapper<Pageable<SearchResult<LearnerDataTransferObject>>, Learners>
{
    private readonly IMapper<LearnerDataTransferObject, Learner> _searchResultToLearnerMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageableSearchResultsToLearnerResultsMapper"/> class.
    /// </summary>
    /// <param name="searchResultToLearnerMapper">
    /// Mapper used to convert individual <see cref="LearnerDataTransferObject"/> documents
    /// into <see cref="Learner"/> domain objects.
    /// </param>
    public PageableSearchResultsToLearnerResultsMapper(
        IMapper<LearnerDataTransferObject, Learner> searchResultToLearnerMapper)
    {
        _searchResultToLearnerMapper = searchResultToLearnerMapper;
    }

    /// <summary>
    /// Maps a pageable collection of Azure Search results into a <see cref="Learners"/> domain model.
    /// </summary>
    /// <param name="input">The pageable search results to map.</param>
    /// <returns>A populated <see cref="Learners"/> instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a search result contains a null document.
    /// </exception>
    public Learners Map(Pageable<SearchResult<LearnerDataTransferObject>> input)
    {
        ArgumentNullException.ThrowIfNull(input);

        Learners learners = new();

        if (input.Any())
        {
            IEnumerable<Learner> mappedResults =
                input.Select(result =>
                    result.Document != null
                        ? _searchResultToLearnerMapper.Map(result.Document)
                        : throw new InvalidOperationException(
                            "Search result document object cannot be null."));

            learners = new Learners(mappedResults);
        }

        return learners;
    }
}
