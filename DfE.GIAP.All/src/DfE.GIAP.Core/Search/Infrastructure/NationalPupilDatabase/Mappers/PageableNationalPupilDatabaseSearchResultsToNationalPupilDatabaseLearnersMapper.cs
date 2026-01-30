using Azure;
using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.Core.Search.Infrastructure.NationalPupilDatabase.DataTransferObjects;

namespace DfE.GIAP.Core.Search.Infrastructure.NationalPupilDatabase.Mappers;
internal sealed class PageableNationalPupilDatabaseSearchResultsToNationalPupilDatabaseLearnersMapper : IMapper<Pageable<SearchResult<NationalPupilDatabaseLearnerDataTransferObject>>, NationalPupilDatabaseLearners>
{
    private readonly IMapper<NationalPupilDatabaseLearnerDataTransferObject, NationalPupilDatabaseLearner> _searchResultToLearnerMapper;

    public PageableNationalPupilDatabaseSearchResultsToNationalPupilDatabaseLearnersMapper(
        IMapper<NationalPupilDatabaseLearnerDataTransferObject, NationalPupilDatabaseLearner> searchResultToLearnerMapper)
    {
        ArgumentNullException.ThrowIfNull(searchResultToLearnerMapper);
        _searchResultToLearnerMapper = searchResultToLearnerMapper;
    }
    public NationalPupilDatabaseLearners Map(Pageable<SearchResult<NationalPupilDatabaseLearnerDataTransferObject>> input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (!input.Any())
        {
            return NationalPupilDatabaseLearners.CreateEmpty();
        }

        IEnumerable<NationalPupilDatabaseLearner>
                mappedResults =
                    input.Select((result) =>
                        result.Document != null ?
                            _searchResultToLearnerMapper.Map(result.Document) :
                                throw new InvalidOperationException("Search result document object cannot be null."));

        return new NationalPupilDatabaseLearners(mappedResults);
    }
}
