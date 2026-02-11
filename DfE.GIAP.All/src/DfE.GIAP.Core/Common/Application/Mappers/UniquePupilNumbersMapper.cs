using DfE.GIAP.Core.Common.Application.ValueObjects;

namespace DfE.GIAP.Core.Common.Application.Mappers;
internal sealed class UniquePupilNumbersMapper : IMapper<IEnumerable<string>, UniquePupilNumbers>
{
    public UniquePupilNumbers Map(IEnumerable<string> input)
    {
        IEnumerable<UniquePupilNumber> upns =
            input?
                .Where(t => UniquePupilNumber.TryCreate(t, out UniquePupilNumber? upn) && upn is not null)
                .Select((validatedUpn) => new UniquePupilNumber(validatedUpn)) ?? [];

        return UniquePupilNumbers.Create(upns);
    }
}
