using Azure;
using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.Core.Search.Infrastructure.FurtherEducation.DataTransferObjects;

namespace DfE.GIAP.Core.Search.Infrastructure.FurtherEducation.Mappers;

/// <summary>
/// Maps a pageable collection of Azure Search results containing <see cref="FurtherEducationLearnerDataTransferObject"/>
/// into a domain model <see cref="FurtherEducationLearners"/> collection.
/// </summary>
public sealed class PageableFurtherEducationSearchResultsToLearnerResultsMapper :
    IMapper<Pageable<SearchResult<FurtherEducationLearnerDataTransferObject>>, FurtherEducationLearners>
{
    private readonly IMapper<FurtherEducationLearnerDataTransferObject, FurtherEducationLearner> _searchResultToLearnerMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageableFurtherEducationSearchResultsToLearnerResultsMapper"/> class.
    /// </summary>
    /// <param name="searchResultToLearnerMapper">
    /// Mapper used to convert individual <see cref="FurtherEducationLearnerDataTransferObject"/> documents
    /// into <see cref="FurtherEducationLearner"/> domain objects.
    /// </param>
    public PageableFurtherEducationSearchResultsToLearnerResultsMapper(
        IMapper<FurtherEducationLearnerDataTransferObject, FurtherEducationLearner> searchResultToLearnerMapper)
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
                input.Select(result =>
                {
                    if (result == null || result.Document == null)
                    {
                        return null;
                    }

                    try
                    {
                        return _searchResultToLearnerMapper.Map(result.Document);
                    }
                    catch
                    {
                        return null;
                    }
                })
                .Where(learner => learner != null)
                .ToList()!;

            learners = new FurtherEducationLearners(mappedResults);
        }

        return learners;
    }
}
