using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;

namespace DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.SearchByUniquePupilNumber;
public record NationalPupilDatabaseSearchByUniquePupilNumberResponse
{
    internal NationalPupilDatabaseSearchByUniquePupilNumberResponse(NationalPupilDatabaseLearners? learners, int? results)
    {
        LearnerSearchResults = learners ?? NationalPupilDatabaseLearners.CreateEmpty();
        TotalNumberOfResults = new(results);
    }

    public NationalPupilDatabaseLearners LearnerSearchResults { get; }

    public SearchResultCount TotalNumberOfResults { get; }
}
