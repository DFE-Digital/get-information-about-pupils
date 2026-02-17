using DfE.GIAP.Core.Search.Application.Services;

namespace DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
public record NationalPupilDatabaseLearners : IHasSearchResults
{
    public NationalPupilDatabaseLearners() : this([])
    {

    }

    public NationalPupilDatabaseLearners(IEnumerable<NationalPupilDatabaseLearner> learners)
    {
        Values = (learners?.ToList() ?? []).AsReadOnly();
    }

    public IReadOnlyCollection<NationalPupilDatabaseLearner> Values { get; }

    public int Count => Values.Count;

    public static NationalPupilDatabaseLearners Create(IEnumerable<NationalPupilDatabaseLearner> learners) => new(learners);
    public static NationalPupilDatabaseLearners CreateEmpty() => new();
}
