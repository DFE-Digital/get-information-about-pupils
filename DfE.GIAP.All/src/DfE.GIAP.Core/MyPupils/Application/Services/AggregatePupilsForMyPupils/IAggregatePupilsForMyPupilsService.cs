using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.QueryModel;
using DfE.GIAP.Core.MyPupils.Domain.Entities;

namespace DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils;
public interface IAggregatePupilsForMyPupilsApplicationService
{
    Task<IEnumerable<Pupil>> GetPupilsAsync(
        UniquePupilNumbers uniquePupilNumbers,
        MyPupilsQueryModel? query,
        CancellationToken ctx = default);
}
