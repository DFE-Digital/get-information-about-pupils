using System.Numerics;
using System.Text;

namespace DfE.GIAP.Verification.Anonymisation;

/// <summary>
/// A fixed-length identifier format with an alphabet for each position, written like a simple regular expression,
/// e.g. <c>[0-9A-F]{18}</c> or <c>[G-SU-W][0-9A-F]{17}</c>.
/// </summary>
/// <remarks>
/// Only character classes (<c>[...]</c>, with ranges), literal characters and <c>{n}</c> repeats are supported. Each
/// value of the format is numbered, so it can be permuted.
/// </remarks>
public sealed class IdentifierFormat
{
    private readonly string[] _alphabets;

    private IdentifierFormat(string pattern, string[] alphabets)
    {
        Pattern = pattern;
        _alphabets = alphabets;
        Size = alphabets.Aggregate(BigInteger.One, (size, alphabet) => size * alphabet.Length);
    }

    public string Pattern { get; }

    public int Length => _alphabets.Length;

    /// <summary>
    /// How many values the format has.
    /// </summary>
    public BigInteger Size { get; }

    public IReadOnlyList<string> Alphabets => _alphabets;

    public static IdentifierFormat Parse(string pattern)
    {
        List<string> alphabets = [];
        int i = 0;

        while (i < pattern.Length)
        {
            string alphabet;
            if (pattern[i] == '[')
            {
                int end = pattern.IndexOf(']', i);
                if (end < 0)
                {
                    throw new FormatException($"'{pattern}' has a [ with no ].");
                }

                alphabet = ParseClass(pattern, pattern[(i + 1)..end]);
                i = end + 1;
            }
            else
            {
                alphabet = pattern[i].ToString();
                i++;
            }

            int repeat = 1;
            if (i < pattern.Length && pattern[i] == '{')
            {
                int end = pattern.IndexOf('}', i);
                if (end < 0 || !int.TryParse(pattern.AsSpan(i + 1, end - i - 1), out repeat) || repeat < 1)
                {
                    throw new FormatException($"'{pattern}' has a repeat that isn't {{n}}.");
                }

                i = end + 1;
            }

            alphabets.AddRange(Enumerable.Repeat(alphabet, repeat));
        }

        if (alphabets.Count == 0)
        {
            throw new FormatException("An identifier format can't be empty.");
        }

        return new IdentifierFormat(pattern, [.. alphabets]);
    }

    public bool Matches(string value)
    {
        if (value.Length != _alphabets.Length)
        {
            return false;
        }

        for (int i = 0; i < value.Length; i++)
        {
            if (!_alphabets[i].Contains(value[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Whether no value can match both formats.
    /// </summary>
    public bool IsDisjointFrom(IdentifierFormat other) =>
        Length != other.Length
        || _alphabets.Zip(other._alphabets).Any(p => !p.First.Intersect(p.Second).Any());

    /// <summary>
    /// The value's number, in <c>[0, Size)</c>.
    /// </summary>
    public BigInteger ToNumber(string value)
    {
        BigInteger number = BigInteger.Zero;
        for (int i = 0; i < _alphabets.Length; i++)
        {
            number = (number * _alphabets[i].Length) + _alphabets[i].IndexOf(value[i], StringComparison.Ordinal);
        }

        return number;
    }

    public string FromNumber(BigInteger number)
    {
        char[] value = new char[_alphabets.Length];
        for (int i = _alphabets.Length - 1; i >= 0; i--)
        {
            value[i] = _alphabets[i][(int)(number % _alphabets[i].Length)];
            number /= _alphabets[i].Length;
        }

        return new string(value);
    }

    private static string ParseClass(string pattern, string characterClass)
    {
        StringBuilder alphabet = new();
        for (int i = 0; i < characterClass.Length; i++)
        {
            if (i + 2 < characterClass.Length && characterClass[i + 1] == '-')
            {
                if (characterClass[i + 2] < characterClass[i])
                {
                    throw new FormatException($"'{pattern}' has a range that's backwards.");
                }

                for (char c = characterClass[i]; c <= characterClass[i + 2]; c++)
                {
                    alphabet.Append(c);
                }

                i += 2;
            }
            else
            {
                alphabet.Append(characterClass[i]);
            }
        }

        string result = new([.. alphabet.ToString().Distinct()]);
        return result.Length == 0 ? throw new FormatException($"'{pattern}' has an empty [].") : result;
    }
}
