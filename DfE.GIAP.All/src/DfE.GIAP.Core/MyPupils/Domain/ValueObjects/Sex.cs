namespace DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
public readonly struct Sex
{
    private readonly char? _value;
    private const char MaleCharacterCode = 'M';
    private const char FemaleCharacterCode = 'F';
    public Sex(string? sexCode) // TODO char
    {
        char? normalisedSexCode = sexCode?.ToLowerInvariant() switch
        {
            "m" or "male" => MaleCharacterCode,
            "f" or "female" => FemaleCharacterCode,
            // TODO is intersex represented?
            _ => null
        };

        // TODO VALIDATE CURRENTLY NOT as LEARNER.Sex which is a string - need to ensure values valid
        //char[] validCodes = [MaleCharacterCode, FemaleCharacterCode];

        //if (!validCodes.Contains(normalisedSexCode))
        //{
        //    throw new ArgumentException($"Invalid character to represent Sex: {normalisedSexCode}");
        //}

        _value = normalisedSexCode;
    }

    public static Sex Male => new(MaleCharacterCode.ToString());
    public static Sex Female => new(FemaleCharacterCode.ToString());
    public override string ToString() => _value.ToString() ?? string.Empty;
}
