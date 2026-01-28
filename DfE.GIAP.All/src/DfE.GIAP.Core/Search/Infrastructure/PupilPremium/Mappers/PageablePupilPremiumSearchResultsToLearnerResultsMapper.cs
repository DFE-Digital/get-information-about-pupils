using Azure;
using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Search.Application.Models.Learner;
using DfE.GIAP.Core.Search.Application.Models.Learner.PupilPremium;
using DfE.GIAP.Core.Search.Infrastructure.PupilPremium.DataTransferObjects;

namespace DfE.GIAP.Core.Search.Infrastructure.PupilPremium.Mappers;

/// <summary>
/// Maps a pageable collection of Azure Search results containing <see cref="PupilPremiumLearnerDataTransferObject"/>
/// into a domain model <see cref="PupilPremiumLearners"/> collection.
/// </summary>
public sealed class PageablePupilPremiumSearchResultsToLearnerResultsMapper :
    IMapper<Pageable<SearchResult<PupilPremiumLearnerDataTransferObject>>, PupilPremiumLearners>
{
    private readonly IMapper<PupilPremiumLearnerDataTransferObject, PupilPremiumLearner> _searchResultToLearnerMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageablePupilPremiumSearchResultsToLearnerResultsMapper"/> class.
    /// </summary>
    /// <param name="searchResultToLearnerMapper">
    /// Mapper used to convert individual <see cref="PupilPremiumLearnerDataTransferObject"/> documents
    /// into <see cref="PupilPremiumLearner"/> domain objects.
    /// </param>
    public PageablePupilPremiumSearchResultsToLearnerResultsMapper(
        IMapper<PupilPremiumLearnerDataTransferObject, PupilPremiumLearner> searchResultToLearnerMapper)
    {
        ArgumentNullException.ThrowIfNull(searchResultToLearnerMapper);
        _searchResultToLearnerMapper = searchResultToLearnerMapper;
    }

    /// <summary>
    /// Maps a pageable collection of Azure Search results into a <see cref="PupilPremiumLearners"/> domain model.
    /// </summary>
    /// <param name="input">The pageable search results to map.</param>
    /// <returns>A populated <see cref="PupilPremiumLearners"/> instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a search result contains a null document.
    /// </exception>
    public PupilPremiumLearners Map(Pageable<SearchResult<PupilPremiumLearnerDataTransferObject>> input)
    {
        ArgumentNullException.ThrowIfNull(input);

        PupilPremiumLearners learners = new();

        if (input.Any())
        {
            IEnumerable<PupilPremiumLearner> mappedResults =
                input.Select(result =>
                    result.Document != null
                        ? _searchResultToLearnerMapper.Map(result.Document)
                        : throw new InvalidOperationException(
                            "Search result document object cannot be null."));

            learners = new PupilPremiumLearners(mappedResults);
        }

        return learners;
    }
}
