namespace DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
public sealed class UniquePupilNumbers
{
    private readonly HashSet<UniquePupilNumber> _uniquePupilNumbers;

    public UniquePupilNumbers(IEnumerable<UniquePupilNumber> uniquePupilNumbers)
    {
        _uniquePupilNumbers = uniquePupilNumbers?.ToHashSet() ?? [];
    }
    public static UniquePupilNumbers Create(IEnumerable<UniquePupilNumber> uniquePupilNumbers) => new(uniquePupilNumbers);

    public int Count => _uniquePupilNumbers.Count;
    public bool IsEmpty => Count == 0;
    public IReadOnlyList<UniquePupilNumber> GetUniquePupilNumbers() => _uniquePupilNumbers.ToList().AsReadOnly();

    public bool Contains(UniquePupilNumber upn)
    {
        if (upn is null)
        {
            return false;
        }

        return _uniquePupilNumbers.Contains(upn);
    }

    public void Add(IEnumerable<UniquePupilNumber> upns)
    {
        ArgumentNullException.ThrowIfNull(upns);
        List<UniquePupilNumber> addUpns = upns.Distinct().ToList();
        addUpns.ToList().ForEach(t => _uniquePupilNumbers.Add(t));
    }

    public void Remove(IEnumerable<UniquePupilNumber> upns)
    {
        ArgumentNullException.ThrowIfNull(upns);
        upns.ToList().ForEach(t => _uniquePupilNumbers.Remove(t));
    }

    public void Clear() => _uniquePupilNumbers.Clear();
}
