using DfE.GIAP.Core.Common.Domain;
using DfE.GIAP.Core.MyPupils.Domain.Exceptions;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Domain.AggregateRoot;
public sealed class MyPupils : AggregateRoot<MyPupilsId>
{
    private readonly UniquePupilNumbers _pupils;
    private readonly int _maxPupilsLimit;

    public MyPupils(
        MyPupilsId identifier,
        UniquePupilNumbers pupils,
        int maxPupilLimit) : base(identifier)
    {
        _pupils = pupils ?? UniquePupilNumbers.Create([]);
        _maxPupilsLimit = maxPupilLimit;

        if (_pupils.Count > maxPupilLimit)
        {
            throw new MyPupilsLimitExceededException(maxPupilLimit);
        }
    }

    public void Add(UniquePupilNumbers addPupilNumbers)
    {
        int newTotal = _pupils.Count + addPupilNumbers.Count;

        if (newTotal > _maxPupilsLimit)
        {
            throw new MyPupilsLimitExceededException(_maxPupilsLimit);
        }

        _pupils.Add(addPupilNumbers.GetUniquePupilNumbers());
    }

    public void DeletePupils(UniquePupilNumbers deletePupilsNumbers)
    {
        if (deletePupilsNumbers is null || deletePupilsNumbers.Count == 0)
        {
            return;
        }

        if (deletePupilsNumbers.GetUniquePupilNumbers().All(deleteUpn => !_pupils.Contains(deleteUpn)))
        {
            throw new ArgumentException($"None of the deleted pupil identifiers are part of the User: {Identifier} MyPupils");
        }

        _pupils.Remove(
            deletePupilsNumbers.GetUniquePupilNumbers());
    }

    public void DeleteAll() => _pupils.Remove(_pupils.GetUniquePupilNumbers());

    public IReadOnlyList<UniquePupilNumber> GetMyPupils() => _pupils.GetUniquePupilNumbers();
}
