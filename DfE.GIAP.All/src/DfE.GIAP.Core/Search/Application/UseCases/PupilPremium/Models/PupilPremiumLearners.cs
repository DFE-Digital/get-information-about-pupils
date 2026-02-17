using DfE.GIAP.Core.Search.Application.Services;

namespace DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
public record PupilPremiumLearners : IHasSearchResults
{
    /// <summary>
    /// Read-only view of the learner results.
    /// Guarantees that consumers cannot modify the internal list.
    /// </summary>
    public IReadOnlyCollection<PupilPremiumLearner> Values { get; }

    public int Count => Values.Count;

    /// <summary>
    /// Default constructor.
    /// Initializes an empty result set.
    /// </summary>
    public PupilPremiumLearners() : this([])
    {

    }

    /// <summary>
    /// Constructs the result set from an incoming sequence of learners.
    /// Uses defensive programming to handle potential null inputs.
    /// </summary>
    /// <param name="learners">Sequence of learners to populate the result set.</param>
    public PupilPremiumLearners(IEnumerable<PupilPremiumLearner> learners)
    {
        Values = (learners?.ToList() ?? []).AsReadOnly();
    }

    /// <summary>
    /// Static factory method to create an empty result set.
    /// Improves readability and conveys intent more clearly than a constructor.
    /// </summary>
    public static PupilPremiumLearners CreateEmpty() => new();
    public static PupilPremiumLearners Create(IEnumerable<PupilPremiumLearner> learners) => new(learners);
}
