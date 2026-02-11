using System.Text;

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

        // Collapse all internal whitespace runs to a single space
        string trimmedInput = CollapseWhitespace(input.Trim());


        if (trimmedInput.Length == 1)
        {
            return char.ToUpperInvariant(trimmedInput[0]).ToString();
        }

        // Include space as a delimiter alongside apostrophe and hyphen
        return NormaliseWithDelimiters(trimmedInput, [' ', '-', '\'']);
    }

    private static string CollapseWhitespace(string value)
    {
        // This simple loop collapses consecutive spaces.
        StringBuilder sb = new(value.Length);

        bool prevWasSpace = false;

        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            if (!char.IsWhiteSpace(c))
            {
                sb.Append(c);
                prevWasSpace = false;
            }
            else
            {
                if (!prevWasSpace)
                {
                    sb.Append(' ');
                    prevWasSpace = true;
                }
            }
        }

        return sb.ToString().Trim();
    }

    private static string NormaliseWithDelimiters(string input, char[] delimiters)
    {
        List<string> result = [];
        
        int start = 0;

        for (int i = 0; i < input.Length; i++)
        {
            char currentCharacter = input[i];
            if (Array.IndexOf(delimiters, currentCharacter) >= 0)
            {
                // Add the part before the delimiter
                result.Add(NormalisePart(input.Substring(start, i - start)));

                // Add the delimiter itself (preserve single spaces/hyphens/apostrophes)
                result.Add(input[i].ToString());

                // Move start past the delimiter
                start = i + 1;
            }
        }

        // Add the last section
        result.Add(NormalisePart(input.Substring(start)));

        return string.Concat(result);
    }

    private static string NormalisePart(string part)
    {
        part = part.Trim();
        if (part.Length == 0) return string.Empty;

        if (part.Length == 1)
        {
            return char.ToUpperInvariant(part[0]).ToString();
        }

        return char.ToUpperInvariant(part[0]) + part.Substring(1).ToLowerInvariant();
    }

}
