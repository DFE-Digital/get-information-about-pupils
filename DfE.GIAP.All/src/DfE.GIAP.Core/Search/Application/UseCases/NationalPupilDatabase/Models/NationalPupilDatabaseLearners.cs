namespace DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
public record NationalPupilDatabaseLearners
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

    public static NationalPupilDatabaseLearners CreateEmpty() => new();
}
