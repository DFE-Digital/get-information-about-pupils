namespace DfE.GIAP.Core.Common.Application.ValueObjects;

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
    public string MiddleNames { get; } // TODO should this be string[]

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

    public LearnerName(string firstName, string? middleName, string surname)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        FirstName = NormaliseName(firstName);

        ArgumentException.ThrowIfNullOrWhiteSpace(surname);
        Surname = NormaliseName(surname);

        // Middlename is optional
        MiddleNames = NormaliseName(middleName);
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
        yield return MiddleNames;
        yield return Surname;
    }

    private static string NormaliseName(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        string trimmedInput = input.Trim();

        if (trimmedInput.Length == 1)
        {
            return char.ToUpperInvariant(trimmedInput[0]).ToString();
        }

        if (!trimmedInput.Contains('-'))
        {
            return char.ToUpperInvariant(trimmedInput[0]) + trimmedInput.Substring(1).ToLowerInvariant();
        }

        // Handle double or triple barrel names
        string[] parts = trimmedInput.Split('-', StringSplitOptions.RemoveEmptyEntries);

        for (int index = 0; index < parts.Length; index++)
        {
            string part = parts[index].Trim();

            if (part.Length == 0)
            {
                continue;
            }

            parts[index] = char.ToUpperInvariant(part[0]) + part.Substring(1).ToLowerInvariant();
        }

        return string.Join("-", parts);
    }
}
