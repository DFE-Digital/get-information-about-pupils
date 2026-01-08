namespace DfE.GIAP.Core.Search.Application.Models.Learner;

/// <summary>
/// Encapsulates the results of a further education learner search.
/// Provides a read-only collection interface and ensures immutability at the boundary.
/// </summary>
public sealed class Learners
{
    // Internal mutable list to hold search results; encapsulated within the class
    private readonly List<Learner> _learners;

    /// <summary>
    /// Read-only view of the learner results.
    /// Guarantees that consumers cannot modify the internal list.
    /// </summary>
    public IReadOnlyCollection<Learner> LearnerCollection => _learners.AsReadOnly();

    /// <summary>
    /// Returns the number of learners in the collection.
    /// Safely handles null by returning 0 if <c>_learners</c> is not initialized.
    /// Supports diagnostic clarity and avoids null reference exceptions during result evaluation.
    /// </summary>
    public int Count => _learners?.Count ?? 0;


    /// <summary>
    /// Default constructor.
    /// Initializes an empty result set.
    /// </summary>
    public Learners()
    {
        _learners = [];
    }

    /// <summary>
    /// Constructs the result set from an incoming sequence of learners.
    /// Uses defensive programming to handle potential null inputs.
    /// </summary>
    /// <param name="learners">Sequence of learners to populate the result set.</param>
    public Learners(IEnumerable<Learner> learners)
    {
        _learners = learners?.ToList() ?? [];
    }

    /// <summary>
    /// Static factory method to create an empty result set.
    /// Improves readability and conveys intent more clearly than a constructor.
    /// </summary>
    public static Learners CreateEmpty() => new();
}

