using Azure;
using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using DfE.GIAP.Core.Search.Infrastructure.PupilPremium.DataTransferObjects;

namespace DfE.GIAP.Core.Search.Infrastructure.PupilPremium.Mappers;

internal sealed class PageablePupilPremiumSearchResultsToLearnerResultsMapper :
    IMapper<Pageable<SearchResult<PupilPremiumLearnerDataTransferObject>>, PupilPremiumLearners>
{
    private readonly IMapperWithResult<PupilPremiumLearnerDataTransferObject, PupilPremiumLearner> _searchResultToLearnerMapper;

    public PageablePupilPremiumSearchResultsToLearnerResultsMapper(
        IMapperWithResult<PupilPremiumLearnerDataTransferObject, PupilPremiumLearner> searchResultToLearnerMapper)
    {
        ArgumentNullException.ThrowIfNull(searchResultToLearnerMapper);
        _searchResultToLearnerMapper = searchResultToLearnerMapper;
    }
    public PupilPremiumLearners Map(Pageable<SearchResult<PupilPremiumLearnerDataTransferObject>> input)
    {
        ArgumentNullException.ThrowIfNull(input);

        PupilPremiumLearners learners = new();

        if (input.Any())
        {
            List<PupilPremiumLearner> mappedResults =
                input
                    .Where(t => t != null && t.Document != null)
                    .Select(result => _searchResultToLearnerMapper.Map(result.Document))
                    .Where(response => response.HasResult)
                    .Select(resultLearner => resultLearner.Result!)
                    .ToList();

            learners = new PupilPremiumLearners(mappedResults);
        }

        return learners;
    }
}
