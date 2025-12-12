using DfE.GIAP.Core.Common.Domain;

namespace DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
public sealed class PupilName : ValueObject<PupilName>
{
    // TODO double barrelled names should we expect normalised in data?
    // TODO middlenames required for Search
    public PupilName(string firstName, string lastName)
    {
        // TODO should we enforce these must exist? or just normalise
        //ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        //ArgumentException.ThrowIfNullOrWhiteSpace(lastName); 
        Forename = Normalise(firstName);
        Surname = Normalise(lastName);
    }

    public string Forename { get; }
    public string Surname { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Forename;
        yield return Surname;
    }

    private static string Normalise(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        string trimmedInput = input.Trim();

        if(trimmedInput.Length == 1)
        {
            return char.ToUpperInvariant(trimmedInput[0]).ToString();
        }

        return char.ToUpperInvariant(trimmedInput[0]) + trimmedInput.Substring(1);        
    }
}
