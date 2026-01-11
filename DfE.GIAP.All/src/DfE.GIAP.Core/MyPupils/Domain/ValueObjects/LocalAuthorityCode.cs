namespace DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
// See https://www.get-information-schools.service.gov.uk/Guidance/LaNameCodes
public readonly struct LocalAuthorityCode
{
    public LocalAuthorityCode(int code)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(code);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(code, 999);
        Code = code;
    }
    public int Code { get; }
}
