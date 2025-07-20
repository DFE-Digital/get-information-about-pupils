using DfE.GIAP.Core.Common.Domain;

namespace DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
public sealed class PupilName : ValueObject<PupilName>
{
    // TODO double barrelled names are they normalised in data and treated singular?
    // TODO middlenames required for Search representation
    public PupilName(string firstName, string lastName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName); // TODO is it possible for these to not exist?
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        FirstName = Normalise(firstName);
        Surname = Normalise(lastName);
    }

    public string FirstName { get; }
    public string Surname { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstName;

        yield return Surname;
    }

    private static string Normalise(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        string trimmedInput = input.Trim();
        return char.ToUpperInvariant(trimmedInput[0]) + trimmedInput.Substring(1).ToLowerInvariant();

    }
}
