namespace DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
public readonly struct Sex
{
    // TODO should we accept "male", "female"
    // TODO is intersex represented?
    private readonly char _value;
    private static readonly char s_maleCharacterCode = 'M';
    private static readonly char s_femaleCharacterCode = 'F';
    public Sex(char sexCode)
    {
        char normalisedSexCode = sexCode switch
        {
            'm' => s_maleCharacterCode,
            'f' => s_femaleCharacterCode,
            _ => sexCode
        };

        char[] validCodes = [s_maleCharacterCode, s_femaleCharacterCode];

        if (!validCodes.Contains(normalisedSexCode))
        {
            throw new ArgumentException($"Invalid character to represent Sex: {normalisedSexCode}");
        }
        _value = normalisedSexCode;
    }

    public static Sex Male => new(s_maleCharacterCode);
    public static Sex Female => new(s_femaleCharacterCode);

    public char AsSingleCharacter() => _value;

}
