using System.Globalization;

namespace DfE.GIAP.Core.Common.Domain;
// See https://www.get-information-schools.service.gov.uk/Guidance/LaNameCodes
public readonly struct LocalAuthorityCode
{
    public LocalAuthorityCode(int code)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(code);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(code, 999);
        Code = code;
    }

    public LocalAuthorityCode(string code) : this(Parse(code))
    {
    }


    private static int Parse(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        if (!int.TryParse(code.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out int parsed))
        {
            throw new ArgumentException("Local authority code must be numeric.", nameof(code));
        }

        return parsed;
    }

    public int Code { get; }
}
