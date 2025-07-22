namespace DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
public readonly struct Sex
{
    // TODO should we accept "male", "female"
    // TODO is intersex represented?

    private readonly char _value;
    private const char MaleCharacterCode = 'M';
    private const char FemaleCharacterCode = 'F';
    public Sex(char sexCode)
    {
        char normalisedSexCode = sexCode switch
        {
            'm' => MaleCharacterCode,
            'f' => FemaleCharacterCode,
            _ => sexCode
        };

        char[] validCodes = [MaleCharacterCode, FemaleCharacterCode];

        if (!validCodes.Contains(normalisedSexCode))
        {
            throw new ArgumentException($"Invalid character to represent Sex: {normalisedSexCode}");
        }

        _value = normalisedSexCode;
    }

    public static Sex Male => new(MaleCharacterCode);
    public static Sex Female => new(FemaleCharacterCode);
    public override string ToString() => _value.ToString();
}
