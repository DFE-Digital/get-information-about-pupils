using System.Text.RegularExpressions;

namespace DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
public static class UniquePupilNumberValidator
{
    /*
        * UPN Format (Unified):
        * Matches either:
        *   - Type 1: 1 letter + 12 digits
        *   - Type 2: 1 letter + 11 digits + 1 letter
        *
        * Regex Explanation:
        * ^[A-HJ-NP-RT-Z]         → First character must be an uppercase letter, excluding I, O, Q, U
        * [0-9]{11}               → Followed by exactly 11 digits
        * ([0-9]|[A-HJ-NP-RT-Z])$ → Ends with either a digit (Type 1) or a valid uppercase letter (Type 2)
        *
        * This is a union of:
        *   REGEX_UPN_TYPE_1 = "^[A-HJ-NP-RT-Z][0-9]{12}$"
        *   REGEX_UPN_TYPE_2 = "^[A-HJ-NP-RT-Z][0-9]{11}[A-HJ-NP-RT-Z]$"
    */

    internal static readonly Regex UPN_TYPE_1 = new("^[A-HJ-NP-RT-Z][0-9]{12}$", RegexOptions.Compiled);
    internal static readonly Regex UPN_TYPE_2 = new("^[A-HJ-NP-RT-Z][0-9]{11}[A-HJ-NP-RT-Z]$", RegexOptions.Compiled);

    public static bool Validate(string? input)
    {
        return !string.IsNullOrWhiteSpace(input) &&
            (UPN_TYPE_1.IsMatch(input) || UPN_TYPE_2.IsMatch(input));
    }
}
