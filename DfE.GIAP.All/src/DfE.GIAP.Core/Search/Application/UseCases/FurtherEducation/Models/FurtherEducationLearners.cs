using DfE.GIAP.Core.Search.Application.Services;

namespace DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;

public sealed class FurtherEducationLearners : IHasSearchResults
{
    public FurtherEducationLearners() : this([])
    {
    }

    public FurtherEducationLearners(IEnumerable<FurtherEducationLearner> learners)
    {
        Learners = (learners?.ToList() ?? []).AsReadOnly();
    }

    public IReadOnlyCollection<FurtherEducationLearner> Learners { get; }

    public int Count => Learners?.Count ?? 0;

    public static FurtherEducationLearners CreateEmpty() => new();
}

