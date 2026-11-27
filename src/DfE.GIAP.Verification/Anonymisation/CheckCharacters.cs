namespace DfE.GIAP.Verification.Anonymisation;

/// <summary>
/// The check characters of UPNs and ULNs.
/// </summary>
public static class CheckCharacters
{
    /// <summary>
    /// The letters a UPN's check letter, and a temporary UPN's last character, can be. I, O and S aren't used.
    /// </summary>
    public const string UpnLetters = "ABCDEFGHJKLMNPQRTUVWXYZ";

    /// <summary>
    /// A UPN's check letter, from its other 12 characters.
    /// </summary>
    /// <remarks>
    /// Each of characters 2 to 13 is multiplied by its position, and the sum's remainder when divided by 23 is the check
    /// letter's index in <see cref="UpnLetters"/>. A temporary UPN's last character counts as its own index.
    /// </remarks>
    /// <returns><see langword="null"/> if the characters aren't 11 digits followed by a digit or a UPN letter.</returns>
    public static char? UpnCheckLetter(ReadOnlySpan<char> characters)
    {
        if (characters.Length != 12)
        {
            return null;
        }

        int sum = 0;
        for (int i = 0; i < 12; i++)
        {
            int value;
            if (char.IsAsciiDigit(characters[i]))
            {
                value = characters[i] - '0';
            }
            else if (i == 11 && UpnLetters.Contains(characters[i]))
            {
                value = UpnLetters.IndexOf(characters[i]);
            }
            else
            {
                return null;
            }

            sum += value * (i + 2);
        }

        return UpnLetters[sum % 23];
    }

    public static bool IsValidUpn(string value) =>
        value.Length == 13 && UpnCheckLetter(value.AsSpan(1)) == value[0];

    /// <summary>
    /// A ULN's check digit, from its first 9 digits, as the web app checks it.
    /// </summary>
    /// <remarks>
    /// The digits are multiplied by 10 down to 2, and the check digit is 10 less the sum's remainder when divided by
    /// 11. A remainder of 0 has no check digit, so no ULN starts with those 9 digits.
    /// </remarks>
    /// <returns><see langword="null"/> if there's no check digit for the digits.</returns>
    public static char? UlnCheckDigit(ReadOnlySpan<char> digits)
    {
        if (digits.Length != 9)
        {
            return null;
        }

        int sum = 0;
        for (int i = 0; i < 9; i++)
        {
            if (!char.IsAsciiDigit(digits[i]))
            {
                return null;
            }

            sum += (digits[i] - '0') * (10 - i);
        }

        int remainder = sum % 11;
        return remainder == 0 ? null : (char)('0' + (10 - remainder));
    }

    public static bool IsValidUln(string value) =>
        value.Length == 10 && value[0] != '0' && UlnCheckDigit(value.AsSpan(0, 9)) == value[9];
}
