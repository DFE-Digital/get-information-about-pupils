using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.Extensions;
public static class UniquePupilNumberExtensions
{
    public static IEnumerable<UniquePupilNumber> ToUniquePupilNumbers(this IEnumerable<string> inputs)
    {
        IEnumerable<UniquePupilNumber> upns =
            inputs.Where((upn) => TryCreateUpn(upn) is not null)
                .Select((validatedUpn) => new UniquePupilNumber(validatedUpn));

        return upns;
    }

    private static UniquePupilNumber? TryCreateUpn(string id) =>
        UniquePupilNumber.TryCreate(id, out UniquePupilNumber? upn) ? upn : null;
}
