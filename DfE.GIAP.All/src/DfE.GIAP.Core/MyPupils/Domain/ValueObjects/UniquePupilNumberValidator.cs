using System.Text.RegularExpressions;

namespace DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
public static partial class UniquePupilNumberValidator
{
    [GeneratedRegex("^[AT]\\d{11}[A-Z0-9]$", RegexOptions.Compiled)]
    private static partial Regex UpnRegex(); // As per https://www.gov.uk/government/publications/unique-pupil-numbers

    public static bool Validate(string? upn)
    {
        if (string.IsNullOrWhiteSpace(upn))
        {
            return false;
        }

        return UpnRegex().IsMatch(upn);
    }
}
