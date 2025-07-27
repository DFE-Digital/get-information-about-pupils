using System.Text.RegularExpressions;

namespace DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
public static partial class UniquePupilNumberValidator
{
    [GeneratedRegex(@"^[A-HJ-NP-RT-Z][0-9]{11}([0-9]|[A-HJ-NP-RT-Z])$", RegexOptions.Compiled)]
    internal static partial Regex UpnRegex();

    // As per https://www.gov.uk/government/publications/unique-pupil-numbers
    /*
     * Explanation:
        ^[A-HJ-NP-RT-Z]: Starts with a valid uppercase letter (excluding I, O, Q, S, U, V).
        [0-9]{11}: Followed by exactly 11 digits.
        ([0-9]|[A-HJ-NP-RT-Z])$: Ends with either a digit (making 12 digits total) or a valid uppercase letter (making 11 digits + 1 letter).
        This unified regex will match both:

        1 letter + 12 digits
        1 letter + 11 digits + 1 letter
     * 
     */ 
    public static bool Validate(string? upn)
    {
        if (string.IsNullOrWhiteSpace(upn))
        {
            return false;
        }

        return UpnRegex().IsMatch(upn);
    }
}
