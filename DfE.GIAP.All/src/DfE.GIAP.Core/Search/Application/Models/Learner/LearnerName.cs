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
    /// Gets the learner's middle name.
    /// </summary>
    public string MiddleName { get; }

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
    public LearnerName(string firstName, string surname) : this(firstName, string.Empty, surname)
    {

    }

    public LearnerName(string firstName, string middleName, string surname)
    {

        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        FirstName = firstName;

        ArgumentException.ThrowIfNullOrWhiteSpace(surname);
        Surname = surname;

        MiddleName =
            string.IsNullOrWhiteSpace(middleName) ?
                string.Empty : middleName;
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
        yield return MiddleName;
        yield return Surname;
    }
}
