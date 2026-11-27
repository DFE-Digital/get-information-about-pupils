using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using DfE.GIAP.Core.PupilData;

namespace DfE.GIAP.Verification.Anonymisation;

public enum Sex
{
    Unknown,
    Male,
    Female
}

/// <summary>
/// Replaces personal data with fake values. The same key always gives the same fake values, so a pupil keeps the same
/// fake identifiers, names and DOB in every file.
/// </summary>
/// <remarks>
/// Identifiers are permuted, so two real identifiers never get the same fake one. Names and DOBs are chosen from the
/// pupil's real identifier and the real value, so the same pupil gets the same fake name in every file, but two pupils
/// with the same real name don't get the same fake one. A value that isn't in the format expected is replaced with
/// characters of the same kinds (a digit for a digit, a letter for a letter), so a row the pipelines reject for it is
/// still rejected; it's never given a valid check character.
/// </remarks>
public sealed partial class ValueAnonymiser : IDisposable
{
    private static readonly BigInteger _permanentUpns = BigInteger.Pow(10, 12);
    private static readonly BigInteger _temporaryUpns = BigInteger.Pow(10, 11) * CheckCharacters.UpnLetters.Length;
    private static readonly BigInteger _ulnPrefixes = BigInteger.Pow(10, 9);
    private static readonly BigInteger _guids = BigInteger.One << 128;

    private readonly byte[] _key;
    private readonly NameDictionary _names;
    private readonly IReadOnlyList<(PmrFormat Format, KeyedPermutation Permutation)> _pmrFormats;
    private readonly KeyedPermutation _permanentUpnPermutation;
    private readonly KeyedPermutation _temporaryUpnPermutation;
    private readonly KeyedPermutation _ulnPermutation;
    private readonly KeyedPermutation _guidPermutation;
    private readonly Dictionary<int, KeyedPermutation> _numberPermutations = [];
    private readonly HMACSHA256 _hmac;

    /// <param name="key">The anonymisation key, at least 32 bytes.</param>
    public ValueAnonymiser(byte[] key, IReadOnlyList<PmrFormat> pmrFormats, NameDictionary names)
    {
        if (key.Length < 32)
        {
            throw new ArgumentException("The anonymisation key must be at least 32 bytes.", nameof(key));
        }

        _key = key;
        _names = names;
        _pmrFormats = [.. pmrFormats.Select(f => (f, new KeyedPermutation(DeriveKey($"pmr:{f.Source.Pattern}"), f.Target.Size)))];
        _permanentUpnPermutation = new KeyedPermutation(DeriveKey("upn"), _permanentUpns);
        _temporaryUpnPermutation = new KeyedPermutation(DeriveKey("upn-temporary"), _temporaryUpns);
        _ulnPermutation = new KeyedPermutation(DeriveKey("uln"), _ulnPrefixes);
        _guidPermutation = new KeyedPermutation(DeriveKey("guid"), _guids);
        _hmac = new HMACSHA256(DeriveKey("values"));
    }

    /// <summary>
    /// Whether the transform replaces an identifier or a name, which must never be in the anonymised row.
    /// </summary>
    public static bool IsIdentifying(ColumnTransform transform) =>
        transform is ColumnTransform.Pmr or ColumnTransform.Upn or ColumnTransform.Uln or ColumnTransform.CandidateNumber
            or ColumnTransform.Guid or ColumnTransform.Forename or ColumnTransform.Surname or ColumnTransform.MiddleNames;

    /// <param name="value">The value, trimmed; not empty.</param>
    /// <param name="subject">The real identifier of the row's pupil or learner, if it has one.</param>
    /// <exception cref="AnonymisationException">A PMR isn't in any of the formats in the classification.</exception>
    public string Anonymise(ColumnTransform transform, string value, string? subject, Sex sex) =>
        transform switch
        {
            ColumnTransform.Keep => value,
            ColumnTransform.Pmr => AnonymisePmr(value),
            ColumnTransform.Upn => AnonymiseUpn(value),
            ColumnTransform.Uln => AnonymiseUln(value),
            ColumnTransform.CandidateNumber => AnonymiseNumber(value),
            ColumnTransform.Guid => AnonymiseGuid(value),
            ColumnTransform.Forename or ColumnTransform.MiddleNames => AnonymiseNames(transform, value, subject, sex switch
            {
                Sex.Male => _names.MaleForenames,
                Sex.Female => _names.FemaleForenames,
                _ => _names.AllForenames
            }),
            ColumnTransform.Surname => AnonymiseNames(transform, value, subject, _names.Surnames),
            ColumnTransform.Dob => AnonymiseDob(value, subject),
            ColumnTransform.Postcode => AnonymisePostcode(value),
            ColumnTransform.Address or ColumnTransform.Blank => string.Empty,
            _ => throw new ArgumentOutOfRangeException(nameof(transform))
        };

    public static Sex ParseSex(string? value) =>
        value?.Trim().ToUpperInvariant() switch
        {
            "M" or "1" or "MALE" => Sex.Male,
            "F" or "2" or "FEMALE" => Sex.Female,
            _ => Sex.Unknown
        };

    public void Dispose()
    {
        foreach ((_, KeyedPermutation permutation) in _pmrFormats)
        {
            permutation.Dispose();
        }

        foreach (KeyedPermutation permutation in _numberPermutations.Values)
        {
            permutation.Dispose();
        }

        _permanentUpnPermutation.Dispose();
        _temporaryUpnPermutation.Dispose();
        _ulnPermutation.Dispose();
        _guidPermutation.Dispose();
        _hmac.Dispose();
    }

    /// <summary>
    /// The shape of a value, with its letters as <c>A</c> or <c>a</c> and its digits as <c>9</c>, e.g. <c>A999999999999</c>.
    /// It isn't personal data.
    /// </summary>
    public static string Shape(string value) =>
        string.Create(value.Length, value, (shape, v) =>
        {
            for (int i = 0; i < v.Length; i++)
            {
                shape[i] = char.IsAsciiDigit(v[i]) ? '9' : char.IsAsciiLetterUpper(v[i]) ? 'A' : char.IsAsciiLetterLower(v[i]) ? 'a' : v[i];
            }
        });

    private string AnonymisePmr(string value)
    {
        foreach ((PmrFormat format, KeyedPermutation permutation) in _pmrFormats)
        {
            if (format.Source.Matches(value))
            {
                return format.Target.FromNumber(permutation.Permute(format.Source.ToNumber(value)));
            }
        }

        // A malformed PMR is still a key, so it could share a fake value with a real one
        throw new AnonymisationException($"A PMR shaped {Shape(value)} isn't in any of the PMR formats.");
    }

    private string AnonymiseUpn(string value)
    {
        if (!CheckCharacters.IsValidUpn(value))
        {
            return Garble("upn", value, CheckCharacters.IsValidUpn);
        }

        string fake;
        if (char.IsAsciiDigit(value[12]))
        {
            BigInteger number = _permanentUpnPermutation.Permute(BigInteger.Parse(value.AsSpan(1), CultureInfo.InvariantCulture));
            fake = number.ToString("D12", CultureInfo.InvariantCulture);
        }
        else
        {
            BigInteger number = (BigInteger.Parse(value.AsSpan(1, 11), CultureInfo.InvariantCulture) * CheckCharacters.UpnLetters.Length)
                + CheckCharacters.UpnLetters.IndexOf(value[12]);
            BigInteger permuted = _temporaryUpnPermutation.Permute(number);
            fake = (permuted / CheckCharacters.UpnLetters.Length).ToString("D11", CultureInfo.InvariantCulture)
                + CheckCharacters.UpnLetters[(int)(permuted % CheckCharacters.UpnLetters.Length)];
        }

        return CheckCharacters.UpnCheckLetter(fake) + fake;
    }

    private string AnonymiseUln(string value)
    {
        if (!CheckCharacters.IsValidUln(value))
        {
            return Garble("uln", value, CheckCharacters.IsValidUln);
        }

        BigInteger prefix = _ulnPermutation.Permute(BigInteger.Parse(value.AsSpan(0, 9), CultureInfo.InvariantCulture), IsValidUlnPrefix);
        string fake = prefix.ToString("D9", CultureInfo.InvariantCulture);

        return fake + CheckCharacters.UlnCheckDigit(fake);
    }

    private static bool IsValidUlnPrefix(BigInteger prefix)
    {
        string digits = prefix.ToString("D9", CultureInfo.InvariantCulture);
        return digits[0] != '0' && CheckCharacters.UlnCheckDigit(digits) is not null;
    }

    private string AnonymiseNumber(string value)
    {
        if (!value.All(char.IsAsciiDigit))
        {
            return Garble("number", value, _ => false);
        }

        if (!_numberPermutations.TryGetValue(value.Length, out KeyedPermutation? permutation))
        {
            permutation = new KeyedPermutation(DeriveKey($"number:{value.Length}"), BigInteger.Pow(10, value.Length));
            _numberPermutations[value.Length] = permutation;
        }

        return permutation.Permute(BigInteger.Parse(value, CultureInfo.InvariantCulture)).ToString($"D{value.Length}", CultureInfo.InvariantCulture);
    }

    // Keeps the GUID's case, hyphens and braces
    private string AnonymiseGuid(string value)
    {
        string hex = new([.. value.Where(char.IsAsciiHexDigit)]);
        if (hex.Length != 32 || value.Any(c => !char.IsAsciiHexDigit(c) && c is not ('-' or '{' or '}')))
        {
            return Garble("guid", value, _ => false);
        }

        string fake = _guidPermutation.Permute(BigInteger.Parse("0" + hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture))
            .ToString("X32", CultureInfo.InvariantCulture)[^32..];
        if (value.Any(char.IsAsciiLetterLower))
        {
            fake = fake.ToLowerInvariant();
        }

        StringBuilder result = new(value.Length);
        int next = 0;
        foreach (char c in value)
        {
            result.Append(char.IsAsciiHexDigit(c) ? fake[next++] : c);
        }

        return result.ToString();
    }

    // Each word is replaced with a name, keeping the spaces and hyphens between them and the case
    private string AnonymiseNames(ColumnTransform transform, string value, string? subject, IReadOnlyList<string> names)
    {
        StringBuilder result = new(value.Length);
        int word = 0;

        foreach (Match match in NameWordRegex().Matches(value))
        {
            if (match.Groups["separator"].Success)
            {
                result.Append(match.Value);
                continue;
            }

            int index = (int)(Hash($"{transform}|{subject}|{word++}|{match.Value}") % (ulong)names.Count);
            string name = names[index];
            if (string.Equals(name, match.Value, StringComparison.OrdinalIgnoreCase))
            {
                name = names[(index + 1) % names.Count];
            }

            result.Append(
                match.Value.Any(char.IsLetter) && match.Value.Where(char.IsLetter).All(char.IsUpper) ? name.ToUpperInvariant()
                : match.Value.Where(char.IsLetter).All(char.IsLower) ? name.ToLowerInvariant()
                : name);
        }

        return result.ToString();
    }

    // Days 1 to 12 stay in 1 to 12, and the rest stay above 12, because the old pipeline read slashed dates month-first:
    // a date it could read must stay one it can read, and one it rejected must stay rejected
    private string AnonymiseDob(string value, string? subject)
    {
        if (DataFileReader.ParseDate(value) is not { } date)
        {
            return value;
        }

        int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
        (int first, int last) = date.Day <= 12 ? (1, 12) : (13, daysInMonth);
        int count = last - first + 1;
        int offset = 1 + (int)(Hash($"dob|{subject}|{value}") % (ulong)(count - 1));
        int day = first + ((date.Day - first + offset) % count);

        Match iso = IsoDateRegex().Match(value);
        if (iso.Success)
        {
            return $"{iso.Groups["prefix"].Value}{day:D2}{iso.Groups["rest"].Value}";
        }

        Match slashed = SlashedDateRegex().Match(value);
        string formatted = slashed.Groups["day"].Value.Length == 2 ? day.ToString("D2", CultureInfo.InvariantCulture) : day.ToString(CultureInfo.InvariantCulture);
        return $"{formatted}{slashed.Groups["rest"].Value}";
    }

    private static string AnonymisePostcode(string value)
    {
        Match postcode = PostcodeRegex().Match(value);
        if (postcode.Success)
        {
            return postcode.Groups["outward"].Value;
        }

        return OutwardCodeRegex().IsMatch(value) ? value : string.Empty;
    }

    // Replaces each digit with a digit and each letter with a letter of the same case; isValid stops it from making a
    // value that's valid by chance
    private string Garble(string purpose, string value, Func<string, bool> isValid)
    {
        byte[] hash = _hmac.ComputeHash(Encoding.UTF8.GetBytes($"garble:{purpose}|{value}"));
        char[] result = new char[value.Length];

        for (int i = 0; i < value.Length; i++)
        {
            int random = hash[i % hash.Length] ^ (i / hash.Length);
            result[i] = value[i] switch
            {
                >= '0' and <= '9' => (char)('0' + (random % 10)),
                >= 'A' and <= 'Z' => (char)('A' + (random % 26)),
                >= 'a' and <= 'z' => (char)('a' + (random % 26)),
                _ => value[i]
            };
        }

        string garbled = new(result);
        if (isValid(garbled))
        {
            // Changing one character of a valid UPN or ULN always makes it invalid
            int last = garbled.Length - 1;
            result[last] = char.IsAsciiDigit(result[last]) ? (char)('0' + ((result[last] - '0' + 1) % 10)) : result[last] == 'Z' ? 'A' : (char)(result[last] + 1);
            garbled = new string(result);
        }

        return garbled;
    }

    private ulong Hash(string input) =>
        BitConverter.ToUInt64(_hmac.ComputeHash(Encoding.UTF8.GetBytes(input)), 0);

    private byte[] DeriveKey(string purpose) => HMACSHA256.HashData(_key, Encoding.UTF8.GetBytes(purpose));

    [GeneratedRegex(@"(?<separator>[\s-]+)|[^\s-]+")]
    private static partial Regex NameWordRegex();

    [GeneratedRegex(@"^(?<prefix>\d{4}-\d{2}-)\d{2}(?<rest>.*)$")]
    private static partial Regex IsoDateRegex();

    [GeneratedRegex(@"^(?<day>\d{1,2})(?<rest>/.*)$")]
    private static partial Regex SlashedDateRegex();

    [GeneratedRegex(@"^(?<outward>[A-Za-z]{1,2}[0-9][A-Za-z0-9]?)\s*[0-9][A-Za-z]{2}$")]
    private static partial Regex PostcodeRegex();

    [GeneratedRegex(@"^[A-Za-z]{1,2}[0-9][A-Za-z0-9]?$")]
    private static partial Regex OutwardCodeRegex();
}

/// <summary>
/// A data file can't be anonymised. The message never has any of its values.
/// </summary>
public sealed class AnonymisationException(string message) : Exception(message);
