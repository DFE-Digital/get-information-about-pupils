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

        _pupils.Add(addPupilNumbers);
    }

    public void DeletePupils(UniquePupilNumbers deletePupilUpns)
    {
        if (deletePupilUpns is null || deletePupilUpns.Count == 0)
        {
            throw new ArgumentException("DeletePupilsUpns cannot be null or empty.");
        }

        List<UniquePupilNumber> deleteUpns = [.. deletePupilUpns.GetUniquePupilNumbers()];

        if (!deleteUpns.Any(_pupils.Contains))
        {
            throw new ArgumentException($"None of the DeletePupilUpns are part of User: {Identifier.Value} MyPupils");
        }

        _pupils.Remove(deleteUpns);
    }

    public void DeleteAll() => _pupils.Clear();

    public IReadOnlyList<UniquePupilNumber> GetMyPupils() => _pupils.GetUniquePupilNumbers();
}