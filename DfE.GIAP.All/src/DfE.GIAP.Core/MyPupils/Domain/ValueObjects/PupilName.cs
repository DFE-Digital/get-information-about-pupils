using DfE.GIAP.Core.Common.Domain;

namespace DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
public sealed class PupilName : ValueObject<PupilName>
{
    public PupilName(string firstName, string lastName) : this(firstName, [], lastName)
    { }

    public PupilName(string firstName, IEnumerable<string> middleNames, string lastName) // TODO double barrelled names are they normalised in data and treated singular?
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName); // TODO is it possible for these to not exist?
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        FirstName = Normalise(firstName);
        Surname = Normalise(lastName);
        MiddleNames = middleNames.Select(Normalise)
            .ToArray()
            .AsReadOnly();
    }

    public string FirstName { get; }
    public string Surname { get; }
    public IReadOnlyList<string> MiddleNames { get; }
    public bool HasMiddleNames => MiddleNames.Any();

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstName;

        foreach (string middleName in MiddleNames)
        {
            yield return middleName;
        }

        yield return Surname;
    }

    private static string Normalise(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        string trimmedInput = input.Trim();
        return char.ToUpperInvariant(trimmedInput[0]) + trimmedInput.Substring(1).ToLowerInvariant();

    }
}
