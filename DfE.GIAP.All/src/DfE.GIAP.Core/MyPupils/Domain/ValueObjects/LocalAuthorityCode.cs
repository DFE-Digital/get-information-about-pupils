namespace DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
// See https://www.get-information-schools.service.gov.uk/Guidance/LaNameCodes
public readonly struct LocalAuthorityCode
{
    public LocalAuthorityCode(int code)
    {
        if (code < 0)
        {
            throw new ArgumentException("Local authority must be a positive number");
        }

        if (code > 999)
        {
            throw new ArgumentOutOfRangeException(nameof(code));
        }
        Code = code;
    }
    public int Code { get; }
}
