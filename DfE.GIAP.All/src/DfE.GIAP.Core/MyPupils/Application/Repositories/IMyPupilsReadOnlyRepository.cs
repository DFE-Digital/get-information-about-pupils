using DfE.GIAP.Core.MyPupils.Domain;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.Repositories;
public interface IMyPupilsReadOnlyRepository
{
    Task<MyPupilsAggregate> GetMyPupils(MyPupilsId id);
    Task<MyPupilsAggregate?> GetMyPupilsOrDefaultAsync(MyPupilsId id);
}
