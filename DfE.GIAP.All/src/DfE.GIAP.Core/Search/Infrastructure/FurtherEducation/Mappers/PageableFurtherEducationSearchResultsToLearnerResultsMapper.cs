using Azure;
using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Core.Search.Infrastructure.FurtherEducation.DataTransferObjects;


namespace DfE.GIAP.Core.Search.Infrastructure.FurtherEducation.Mappers;

/// <summary>
/// Maps a pageable collection of Azure Search results containing <see cref="FurtherEducationLearnerDataTransferObject"/>
/// into a domain model <see cref="FurtherEducationLearners"/> collection.
/// </summary>
public sealed class PageableFurtherEducationSearchResultsToLearnerResultsMapper :
    IMapper<Pageable<SearchResult<FurtherEducationLearnerDataTransferObject>>, FurtherEducationLearners>
{
    private readonly IMapperWithResult<FurtherEducationLearnerDataTransferObject, FurtherEducationLearner> _searchResultToLearnerMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageableFurtherEducationSearchResultsToLearnerResultsMapper"/> class.
    /// </summary>
    /// <param name="searchResultToLearnerMapper">
    /// Mapper used to convert individual <see cref="FurtherEducationLearnerDataTransferObject"/> documents
    /// into <see cref="FurtherEducationLearner"/> domain objects.
    /// </param>
    public PageableFurtherEducationSearchResultsToLearnerResultsMapper(
        IMapperWithResult<FurtherEducationLearnerDataTransferObject, FurtherEducationLearner> searchResultToLearnerMapper)
    {
        _searchResultToLearnerMapper = searchResultToLearnerMapper;
    }

    /// <summary>
    /// Maps a pageable collection of Azure Search results into a <see cref="FurtherEducationLearners"/> domain model.
    /// </summary>
    /// <param name="input">The pageable search results to map.</param>
    /// <returns>A populated <see cref="FurtherEducationLearners"/> instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a search result contains a null document.
    /// </exception>
    public FurtherEducationLearners Map(Pageable<SearchResult<FurtherEducationLearnerDataTransferObject>> input)
    {
        ArgumentNullException.ThrowIfNull(input);

        FurtherEducationLearners learners = new();

        if (input.Any())
        {
            List<FurtherEducationLearner> mappedResults =
                input
                    .Where(t => t != null && t.Document != null)
                    .Select(result => _searchResultToLearnerMapper.Map(result.Document))
                    .Where(response => response.HasResult)
                    .Select(resultLearner => resultLearner.Result!)
                    .ToList();

            learners = new FurtherEducationLearners(mappedResults);
        }

        return learners;
    }
}
