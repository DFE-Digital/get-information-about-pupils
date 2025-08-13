using Azure;
using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Search.FurtherEducation.Application.UseCases.SearchByFirstnameAndOrSurname.Models;
using Dto = DfE.GIAP.Core.Search.FurtherEducation.Infrastructure.DataTransferObjects;
using Model = DfE.GIAP.Core.Search.FurtherEducation.Application.UseCases.SearchByFirstnameAndOrSurname.Models;

namespace DfE.GIAP.Core.Search.FurtherEducation.Infrastructure.Mappers;

/// <summary>
/// Maps a pageable collection of Azure Search results containing <see cref="Dto.FurtherEducationLearner"/>
/// into a domain model <see cref="FurtherEducationLearners"/> collection.
/// </summary>
public sealed class PageableSearchResultsToFurtherEducationLearnerResultsMapper :
    IMapper<Pageable<SearchResult<Dto.FurtherEducationLearner>>, FurtherEducationLearners>
{
    private readonly IMapper<Dto.FurtherEducationLearner, FurtherEducationLearner> _searchResultToFurtherEducationPupilMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageableSearchResultsToFurtherEducationLearnerResultsMapper"/> class.
    /// </summary>
    /// <param name="searchResultToFurtherEducationPupilMapper">
    /// Mapper used to convert individual <see cref="Dto.FurtherEducationLearner"/> documents
    /// into <see cref="Model.FurtherEducationLearner"/> domain objects.
    /// </param>
    public PageableSearchResultsToFurtherEducationLearnerResultsMapper(
        IMapper<Dto.FurtherEducationLearner, FurtherEducationLearner> searchResultToFurtherEducationPupilMapper)
    {
        _searchResultToFurtherEducationPupilMapper = searchResultToFurtherEducationPupilMapper;
    }

    /// <summary>
    /// Maps a pageable collection of Azure Search results into a <see cref="FurtherEducationLearners"/> domain model.
    /// </summary>
    /// <param name="input">The pageable search results to map.</param>
    /// <returns>A populated <see cref="FurtherEducationLearners"/> instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a search result contains a null document.
    /// </exception>
    public FurtherEducationLearners Map(Pageable<SearchResult<Dto.FurtherEducationLearner>> input)
    {
        ArgumentNullException.ThrowIfNull(input);

        FurtherEducationLearners furtherEducationPupils = new();

        if (input.Any())
        {
            IEnumerable<FurtherEducationLearner> mappedResults =
                input.Select(result =>
                    result.Document != null
                        ? _searchResultToFurtherEducationPupilMapper.Map(result.Document)
                        : throw new InvalidOperationException(
                            "Search result document object cannot be null."));

            furtherEducationPupils = new FurtherEducationLearners(mappedResults);
        }

        return furtherEducationPupils;
    }
}
