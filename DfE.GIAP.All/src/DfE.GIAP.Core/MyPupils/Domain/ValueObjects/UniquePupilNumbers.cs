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

    public bool Contains(UniquePupilNumber upn)
    {
        if(upn is null)
        {
            return false;
        }

        return _uniquePupilNumbers.Contains(upn);
    }

    public void Add(IEnumerable<UniquePupilNumber> upns)
    {
        ArgumentNullException.ThrowIfNull(upns);
        List<UniquePupilNumber> addUpns = upns.Distinct().ToList();
        _uniquePupilNumbers.AddRange(addUpns);
    }

    public void Remove(IEnumerable<UniquePupilNumber> deleteUpns)
    {
        ArgumentNullException.ThrowIfNull(deleteUpns);
        _uniquePupilNumbers.RemoveAll(
            (currentUpn) => deleteUpns.Contains(currentUpn));
    }
}
