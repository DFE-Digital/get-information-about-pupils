namespace DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
public readonly struct Sex
{
    private readonly char? _value;
    private const char MaleCharacterCode = 'M';
    private const char FemaleCharacterCode = 'F';
    public Sex(string? sexCode)
    {
        char? normalisedSexCode = sexCode?.ToLowerInvariant() switch
        {
            "m" or "male" => MaleCharacterCode,
            "f" or "female" => FemaleCharacterCode,
            // TODO is intersex represented?
            _ => null
        };

#pragma warning disable S125 // Sections of code should not be commented out
        // TODO assumed that it can be null so not validating 
        //char[] validCodes = [MaleCharacterCode, FemaleCharacterCode];
        //if (!validCodes.Contains(normalisedSexCode))
        //{
        //    throw new ArgumentException($"Invalid character to represent Sex: {normalisedSexCode}");
        //}
#pragma warning restore S125 // Sections of code should not be commented out

        _value = normalisedSexCode;
    }

    public static Sex Male => new(MaleCharacterCode.ToString());
    public static Sex Female => new(FemaleCharacterCode.ToString());
    public override string ToString() => _value.ToString() ?? string.Empty;
}
