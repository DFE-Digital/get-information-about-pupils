namespace DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
public sealed class UniquePupilNumbers
{
    private readonly List<UniquePupilNumber> _uniquePupilNumbers;

    public UniquePupilNumbers(IEnumerable<UniquePupilNumber> uniquePupilNumbers)
    {
        _uniquePupilNumbers = uniquePupilNumbers?.ToList() ?? [];
    }
    public static UniquePupilNumbers Create(IEnumerable<UniquePupilNumber> uniquePupilNumbers) => new(uniquePupilNumbers);

    public int Count => _uniquePupilNumbers.Count;
    public bool IsEmpty => Count == 0;
    public IReadOnlyList<UniquePupilNumber> GetUniquePupilNumbers() => _uniquePupilNumbers.AsReadOnly();
    public void Add(IEnumerable<UniquePupilNumber> upns)
    {
        ArgumentNullException.ThrowIfNull(upns);
        _uniquePupilNumbers.AddRange(upns);
    }
}
