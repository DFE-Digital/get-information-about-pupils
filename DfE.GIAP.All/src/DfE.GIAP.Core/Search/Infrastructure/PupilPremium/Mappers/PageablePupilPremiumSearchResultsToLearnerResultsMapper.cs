using Azure;
using Azure.Search.Documents.Models;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using DfE.GIAP.Core.Search.Infrastructure.PupilPremium.DataTransferObjects;

namespace DfE.GIAP.Core.Search.Infrastructure.PupilPremium.Mappers;

public sealed class PageablePupilPremiumSearchResultsToLearnerResultsMapper :
    IMapper<Pageable<SearchResult<PupilPremiumLearnerDataTransferObject>>, PupilPremiumLearners>
{
    private readonly IMapper<PupilPremiumLearnerDataTransferObject, PupilPremiumLearner> _searchResultToLearnerMapper;

    public PageablePupilPremiumSearchResultsToLearnerResultsMapper(
        IMapper<PupilPremiumLearnerDataTransferObject, PupilPremiumLearner> searchResultToLearnerMapper)
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
