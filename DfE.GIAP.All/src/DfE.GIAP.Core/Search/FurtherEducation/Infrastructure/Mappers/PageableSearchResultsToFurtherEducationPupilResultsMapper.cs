using Azure;
using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Search.FurtherEducation.Application.UseCases.SearchByFirstnameAndOrSurname.Models;
using Dto = DfE.GIAP.Core.Search.FurtherEducation.Infrastructure.DataTransferObjects;
using Model = DfE.GIAP.Core.Search.FurtherEducation.Application.UseCases.SearchByFirstnameAndOrSurname.Models;

namespace DfE.GIAP.Core.Search.FurtherEducation.Infrastructure.Mappers;

/// <summary>
/// 
/// </summary>
public sealed class PageableSearchResultsToFurtherEducationPupilResultsMapper : IMapper<Pageable<SearchResult<Dto.FurtherEducationPupil>>, Model.FurtherEducationPupils>
{
    private readonly IMapper<Dto.FurtherEducationPupil, Model.FurtherEducationPupil> _searchResultToFurtherEducationPupilMapper;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="searchResultToFurtherEducationPupilMapper"></param>
    public PageableSearchResultsToFurtherEducationPupilResultsMapper(IMapper<Dto.FurtherEducationPupil, Model.FurtherEducationPupil> searchResultToFurtherEducationPupilMapper)
    {
        _searchResultToFurtherEducationPupilMapper = searchResultToFurtherEducationPupilMapper;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public FurtherEducationPupils Map(Pageable<SearchResult<Dto.FurtherEducationPupil>> input)
    {
        ArgumentNullException.ThrowIfNull(input);
        FurtherEducationPupils furtherEducationPupils = new();

        if (input.Any())
        {
            IEnumerable<FurtherEducationPupil> mappedResults =
                input.Select(result =>
                    result.Document != null
                    ? _searchResultToFurtherEducationPupilMapper.Map(result.Document)
                    : throw new InvalidOperationException(
                        "Search result document object cannot be null."));

            furtherEducationPupils =
                new FurtherEducationPupils(mappedResults);
        }

        return furtherEducationPupils;
    }
}
