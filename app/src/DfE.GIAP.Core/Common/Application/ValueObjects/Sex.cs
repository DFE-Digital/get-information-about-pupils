namespace DfE.GIAP.Core.Common.Application.ValueObjects;

public readonly record struct Sex : IEquatable<Sex>, IComparable<Sex>
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

    public override string ToString()
    {
        return _code switch
        {
            FemaleCode => "F",
            MaleCode => "M",
            _ => "U"
        };
    }

    public int CompareTo(Sex other)
    {
        return GetRank(_code)
                .CompareTo(
                    GetRank(other._code));
    }

    private static int GetRank(char c) => c switch
    {
        FemaleCode => 0,
        MaleCode => 1,
        _ => 2
    };


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
