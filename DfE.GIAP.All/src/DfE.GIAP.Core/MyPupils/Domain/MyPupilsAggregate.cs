using DfE.GIAP.Core.Common.Domain;
using DfE.GIAP.Core.MyPupils.Domain.Exceptions;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Domain;
public sealed class MyPupilsAggregate : AggregateRoot<MyPupilsId>
{
    private readonly UniquePupilNumbers _pupils;
    private readonly int _maxPupilsLimit;

    public MyPupilsAggregate(
        MyPupilsId identifier,
        UniquePupilNumbers pupils,
        int maxPupilsLimit) : base(identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        _pupils = pupils ?? UniquePupilNumbers.Empty();

        _maxPupilsLimit = maxPupilsLimit;

        if (_pupils.Count > _maxPupilsLimit)
        {
            throw new MyPupilsLimitExceededException(maxPupilsLimit);
        }
    }

    public int PupilCount => _pupils.Count;

    public bool HasNoPupils => PupilCount == 0;

    public void AddPupils(UniquePupilNumbers addPupilNumbers)
    {
        if ((_pupils.Count + addPupilNumbers.Count) > _maxPupilsLimit)
        {
            throw new MyPupilsLimitExceededException(_maxPupilsLimit);
        }

        _pupils.Add(addPupilNumbers.GetUniquePupilNumbers());
    }

    public void DeletePupils(UniquePupilNumbers deletePupilsNumbers)
    {
        if (deletePupilsNumbers is null || deletePupilsNumbers.Count == 0)
        {
            return; // TODO consider throw?
        }

        List<UniquePupilNumber> deleteUpns = deletePupilsNumbers.GetUniquePupilNumbers().ToList();

        if (deleteUpns.All(deleteUpn => !_pupils.Contains(deleteUpn)))
        {
            throw new ArgumentException($"None of the deleted pupil identifiers are part of the User: {Identifier} MyPupils");
        }

        _pupils.Remove(deleteUpns);
    }

    public void DeleteAll() => _pupils.Clear();

    public IReadOnlyList<UniquePupilNumber> GetMyPupils() => _pupils.GetUniquePupilNumbers();
}
