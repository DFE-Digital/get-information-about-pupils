namespace DfE.GIAP.Core.Search.FurtherEducation.Application.UseCases.SearchByFirstnameAndOrSurname.Models;

/// <summary>
/// Encapsulates the results of a further education learner search.
/// Provides a read-only collection interface and ensures immutability at the boundary.
/// </summary>
public sealed class FurtherEducationLearners
{
    // Internal mutable list to hold search results; encapsulated within the class
    private readonly List<FurtherEducationLearner> _learners;

    /// <summary>
    /// Read-only view of the learner results.
    /// Guarantees that consumers cannot modify the internal list.
    /// </summary>
    public IReadOnlyCollection<FurtherEducationLearner> Learners => _learners.AsReadOnly();

    /// <summary>
    /// Gets the total number of learners in the results.
    /// Useful for quick metrics or UI display.
    /// </summary>
    public int Count => _learners.Count;

    /// <summary>
    /// Default constructor.
    /// Initializes an empty result set.
    /// </summary>
    public FurtherEducationLearners()
    {
        _learners = [];
    }

    /// <summary>
    /// Constructs the result set from an incoming sequence of learners.
    /// Uses defensive programming to handle potential null inputs.
    /// </summary>
    /// <param name="furtherEducationPupils">Sequence of learners to populate the result set.</param>
    public FurtherEducationLearners(IEnumerable<FurtherEducationLearner> furtherEducationPupils)
    {
        _learners = furtherEducationPupils?.ToList() ?? [];
    }

    /// <summary>
    /// Static factory method to create an empty result set.
    /// Improves readability and conveys intent more clearly than a constructor.
    /// </summary>
    public static FurtherEducationLearners CreateEmpty() => new();
}

