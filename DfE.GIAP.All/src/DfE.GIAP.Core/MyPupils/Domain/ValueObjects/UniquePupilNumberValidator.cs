using System.Text.RegularExpressions;

namespace DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
public static class UniquePupilNumberValidator
{
    // see GOV.UK spec
    internal static readonly Regex UPN_TYPE_1 = new("^[A-HJ-NP-RT-Z][0-9]{12}$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(200));
    internal static readonly Regex UPN_TYPE_2 = new("^[A-HJ-NP-RT-Z][0-9]{11}[A-HJ-NP-RT-Z]$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(200));

    public static bool Validate(string? input)
    {
        return !string.IsNullOrWhiteSpace(input) &&
            (UPN_TYPE_1.IsMatch(input) || UPN_TYPE_2.IsMatch(input));
    }
}
