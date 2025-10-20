using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.Extensions;
public static class UniquePupilNumberExtensions
{
    public static List<UniquePupilNumber> ToUniquePupilNumbers(this IEnumerable<string> inputs)
    {
        List<UniquePupilNumber> upns =
            inputs.Where((upn) => TryCreateUpn(upn) is not null)
                .Select((validatedUpn) => new UniquePupilNumber(validatedUpn))
                .ToList();

        return upns;
    }

    private static UniquePupilNumber? TryCreateUpn(string id) =>
        UniquePupilNumber.TryCreate(id, out UniquePupilNumber? upn) ? upn : null;
}
