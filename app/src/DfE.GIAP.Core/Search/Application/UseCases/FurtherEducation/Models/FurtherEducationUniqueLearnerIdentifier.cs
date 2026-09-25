namespace DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;

/// <summary>
/// Represents a learner's unique identifier (ULN) as a value object.
/// Ensures the ULN is a valid 10-digit integer.
/// </summary>
public sealed class FurtherEducationUniqueLearnerIdentifier : ValueObject<FurtherEducationUniqueLearnerIdentifier>
{
    /// <summary>
    /// Gets the parsed Unique Learner Number (ULN) as an integer.
    /// </summary>
    public long UniqueLearnerNumber { get; }

    /// <summary>
    /// Constructs a new <see cref="FurtherEducationUniqueLearnerIdentifier"/> from a string input.
    /// Validates that the input is a non-null, 10-digit numeric string.
    /// </summary>
    /// <param name="uniqueLearnerNumber">The ULN string to validate and parse.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the input is null, not 10 digits, or not numeric.
    /// </exception>
    public FurtherEducationUniqueLearnerIdentifier(string uniqueLearnerNumber)
    {
        // Check for null, empty, or whitespace input.
        if (string.IsNullOrWhiteSpace(uniqueLearnerNumber))
            throw new ArgumentException("Unique-Learner-Number (ULN) must not be null or empty.");

        // Ensure the string is exactly 10 characters long.
        if (uniqueLearnerNumber.Length != 10)
            throw new ArgumentException("Unique-Learner-Number (ULN) must be exactly 10 digits long.");

        // Attempt to parse the string to an integer.
        if (!long.TryParse(uniqueLearnerNumber, out long parsedUniqueLearnerNumber))
            throw new ArgumentException("Unique-Learner-Number (ULN) must be a valid numeric value.");

        UniqueLearnerNumber = parsedUniqueLearnerNumber;


        // Assign the parsed value to the property
        UniqueLearnerNumber = parsedUniqueLearnerNumber;
    }

    /// <summary>
    /// Provides the components used for equality comparison.
    /// Required by the ValueObject base class.
    /// </summary>
    /// <returns>An enumerable containing the ULN.</returns>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return UniqueLearnerNumber;
    }
}
