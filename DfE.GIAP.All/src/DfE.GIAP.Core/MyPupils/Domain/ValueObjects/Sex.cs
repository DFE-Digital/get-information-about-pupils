namespace DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
public readonly struct Sex
{
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

        // TODO is Intersex represented?
        char[] validCodes = [s_maleCharacterCode, s_femaleCharacterCode];

        if (!validCodes.Contains(sexCode))
        {
            throw new ArgumentException($"Invalid character to represent Sex: {sexCode}");
        }
        _value = normalisedSexCode;
    }

    public char AsSingleCharacter() => _value;
}
