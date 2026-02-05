using Azure;
using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.Core.Search.Infrastructure.NationalPupilDatabase.DataTransferObjects;

namespace DfE.GIAP.Core.Search.Infrastructure.NationalPupilDatabase.Mappers;
internal sealed class PageableNationalPupilDatabaseSearchResultsToNationalPupilDatabaseLearnersMapper : IMapper<Pageable<SearchResult<NationalPupilDatabaseLearnerDataTransferObject>>, NationalPupilDatabaseLearners>
{
    private readonly IMapperWithResult<NationalPupilDatabaseLearnerDataTransferObject, NationalPupilDatabaseLearner> _searchResultToLearnerMapper;

    public PageableNationalPupilDatabaseSearchResultsToNationalPupilDatabaseLearnersMapper(
        IMapperWithResult<NationalPupilDatabaseLearnerDataTransferObject, NationalPupilDatabaseLearner> searchResultToLearnerMapper)
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

        List<NationalPupilDatabaseLearner> mappedResults =
            input.Where(t => t != null && t.Document != null)
                .Select(result => _searchResultToLearnerMapper.Map(result.Document))
                .Where(response => response.HasResult)
                .Select(resultLearner => resultLearner.Result!)
                .ToList();

        return new NationalPupilDatabaseLearners(mappedResults);
    }
}
