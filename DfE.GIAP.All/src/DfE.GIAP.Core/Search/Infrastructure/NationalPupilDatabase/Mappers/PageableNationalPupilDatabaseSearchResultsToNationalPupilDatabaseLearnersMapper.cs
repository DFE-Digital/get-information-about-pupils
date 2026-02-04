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

        List<NationalPupilDatabaseLearner> mappedResults =
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
                catch (Exception)
                {
                    // Swallow mapping errors for individual records; invalid results are skipped.
                    return null;
                }
            })
            .Where(learner => learner != null)
            .ToList()!;


        return new NationalPupilDatabaseLearners(mappedResults);
    }
}
