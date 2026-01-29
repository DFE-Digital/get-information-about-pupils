using DfE.GIAP.Core.Common.ValueObjects;

namespace DfE.GIAP.Core.Common.Domain;
public sealed class UniquePupilNumbers
{
    private readonly HashSet<UniquePupilNumber> _uniquePupilNumbers;

    public UniquePupilNumbers(IEnumerable<UniquePupilNumber> uniquePupilNumbers)
    {
        _uniquePupilNumbers = uniquePupilNumbers?.ToHashSet() ?? [];
    }

    public static UniquePupilNumbers Create(IEnumerable<UniquePupilNumber> uniquePupilNumbers) => new(uniquePupilNumbers);

    public static UniquePupilNumbers Empty() => Create([]);
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

        foreach (UniquePupilNumber upn in addUpns)
        {
            _uniquePupilNumbers.Add(upn);
        }
    }

    public void Add(UniquePupilNumbers uniquePupilNumbers)
    {
        ArgumentNullException.ThrowIfNull(uniquePupilNumbers);
        Add(uniquePupilNumbers.GetUniquePupilNumbers());
    }

    public void Remove(IEnumerable<UniquePupilNumber> upns)
    {
        ArgumentNullException.ThrowIfNull(upns);

        foreach (UniquePupilNumber upn in upns)
        {
            _uniquePupilNumbers.Remove(upn);
        }
    }

    public void Remove(UniquePupilNumbers uniquePupilNumbers)
    {
        ArgumentNullException.ThrowIfNull(uniquePupilNumbers);
        Remove(uniquePupilNumbers.GetUniquePupilNumbers());
    }

    public void Clear() => _uniquePupilNumbers.Clear();
}
