namespace DfE.GIAP.Verification.Anonymisation;

/// <summary>
/// The names that real names are replaced with.
/// </summary>
/// <remarks>
/// The lists in <c>Anonymisation/Names</c> are the English locale's names from Bogus (MIT licence).
/// </remarks>
public sealed record NameDictionary(IReadOnlyList<string> MaleForenames, IReadOnlyList<string> FemaleForenames, IReadOnlyList<string> Surnames)
{
    public IReadOnlyList<string> AllForenames { get; } = [.. MaleForenames, .. FemaleForenames];

    public static NameDictionary Load(string directory) =>
        new(
            ReadNames(Path.Combine(directory, "male-forenames.txt")),
            ReadNames(Path.Combine(directory, "female-forenames.txt")),
            ReadNames(Path.Combine(directory, "surnames.txt")));

    public static NameDictionary LoadShipped() => Load(Path.Combine(AppContext.BaseDirectory, "Anonymisation", "Names"));

    private static string[] ReadNames(string path) =>
        [.. File.ReadAllLines(path).Select(n => n.Trim()).Where(n => n.Length > 0)];
}
