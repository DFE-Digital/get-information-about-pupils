namespace DfE.GIAP.Core.Search.FurtherEducation.Application.UseCases.SearchByFirstnameAndOrSurname.Models;

/// <summary>
/// Encapsulates the results of a further education pupil search.
/// Provides a read-only collection interface and ensures immutability at the boundary.
/// </summary>
public sealed class FurtherEducationPupils
{
    // Internal mutable list to hold search results; encapsulated within the class
    private readonly List<FurtherEducationPupil> _furtherEducationPupils;

    /// <summary>
    /// Read-only view of the pupil results.
    /// Guarantees that consumers cannot modify the internal list.
    /// </summary>
    public IReadOnlyCollection<FurtherEducationPupil> Pupils => _furtherEducationPupils.AsReadOnly();

    /// <summary>
    /// Gets the total number of pupils in the results.
    /// Useful for quick metrics or UI display.
    /// </summary>
    public int Count => _furtherEducationPupils.Count;

    /// <summary>
    /// Default constructor.
    /// Initializes an empty result set.
    /// </summary>
    public FurtherEducationPupils()
    {
        _furtherEducationPupils = [];
    }

    /// <summary>
    /// Constructs the result set from an incoming sequence of pupils.
    /// Uses defensive programming to handle potential null inputs.
    /// </summary>
    /// <param name="furtherEducationPupils">Sequence of pupils to populate the result set.</param>
    public FurtherEducationPupils(IEnumerable<FurtherEducationPupil> furtherEducationPupils)
    {
        _furtherEducationPupils = furtherEducationPupils?.ToList() ?? [];
    }

    /// <summary>
    /// Static factory method to create an empty result set.
    /// Improves readability and conveys intent more clearly than a constructor.
    /// </summary>
    public static FurtherEducationPupils CreateEmpty() => new();
}

