using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.QueryModel;
using DfE.GIAP.Core.MyPupils.Domain.Entities;

namespace DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Handlers;
internal sealed class PaginatePupilsHandler : IPaginatePupilsHandler
{
    public IEnumerable<Pupil> PaginatePupils(IEnumerable<Pupil> pupils, PaginationOptions options)
    {
        if (pupils is null)
        {
            return [];
        }

        if (pupils.TryGetNonEnumeratedCount(out int count) && count == 0)
        {
            return [];
        }

        if (options is null)
        {
            return pupils;
        }

        // Page 1 = (size * 0) = 0 -> Size
        // Page 2 = (size * 1) = Size -> Size*2
        int skip = options.Size * (options.Page.Value - 1);

        if (skip >= pupils.Count())
        {
            return pupils;
        }

        return pupils.Skip(skip).Take(options.Size);
    }
}
