namespace DfE.GIAP.Core.Search.Application.Models.Learner;

/// <summary>
/// Represents a learner's name as a domain value object.
/// Encapsulates first and surname with equality semantics.
/// </summary>
public sealed class LearnerName : ValueObject<LearnerName>
{
    /// <summary>
    /// Gets the learner's first name.
    /// </summary>
    public string FirstName { get; }

    /// <summary>
    /// Gets the learner's surname.
    /// </summary>
    public string Surname { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LearnerName"/> class.
    /// </summary>
    /// <param name="firstName">The learner's first name. Must not be null or whitespace.</param>
    /// <param name="surname">The learner's surname. Must not be null or whitespace.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="firstName"/> or <paramref name="surname"/> is null or whitespace.
    /// </exception>
    public LearnerName(string firstName, string surname)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("The Learner's first name field is required.");
        if (string.IsNullOrWhiteSpace(surname))
            throw new ArgumentException("The Learner's surname field is required.");

        FirstName = firstName;
        Surname = surname;
    }

    /// <summary>
    /// Provides the components used to determine equality between <see cref="LearnerName"/> instances.
    /// </summary>
    /// <returns>
    /// A sequence of objects representing the equality components: <see cref="FirstName"/> and <see cref="Surname"/>.
    /// </returns>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstName;
        yield return Surname;
    }
}
