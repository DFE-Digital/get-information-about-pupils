namespace DfE.GIAP.Core.Common.Application.ValueObjects;

public readonly record struct Sex : IEquatable<Sex>
{
    private const char MaleCode = 'M';
    private const char FemaleCode = 'F';
    private const char UnknownCode = 'U';

    private readonly char _code;

    public Sex(char? code)
    {
        _code = NormalizeChar(code);
    }

    public Sex(string? value)
    {
        _code = NormalizeString(value);
    }

    public static Sex Male => new(MaleCode);
    public static Sex Female => new(FemaleCode);
    public static Sex Unknown => new(UnknownCode);

    public bool IsKnown => _code != UnknownCode;
    public bool IsMale => _code == MaleCode;
    public bool IsFemale => _code == FemaleCode;

    public override string ToString()
    {
        return _code switch
        {
            MaleCode => "M",
            FemaleCode => "F",
            _ => "U"
        };
    }

    public static bool TryParse(string? value, out Sex sex)
    {
        char code = NormalizeString(value);
        sex = new Sex(code);
        return sex.IsKnown;
    }

    public static Sex Parse(string? value)
    {
        if (!TryParse(value, out Sex sex))
        {
            throw new ArgumentException($"Invalid sex value: '{value}'.", nameof(value));
        }
        return sex;
    }

    private static char NormalizeChar(char? c)
    {
        if (c == null || c.Value == default)
        {
            return UnknownCode;
        }

        char u = char.ToUpperInvariant(c.Value);
        return u switch
        {
            MaleCode => MaleCode,
            FemaleCode => FemaleCode,
            _ => UnknownCode
        };
    }

    private static char NormalizeString(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return UnknownCode;
        }

        if (input.Length == 1)
        {
            return NormalizeChar(input[0]);
        }

        // parsing
        string v = input.Trim().ToLowerInvariant();
        return v switch
        {
            "m" or "male" => MaleCode,
            "f" or "female" => FemaleCode,
            _ => UnknownCode
        };
    }
}
