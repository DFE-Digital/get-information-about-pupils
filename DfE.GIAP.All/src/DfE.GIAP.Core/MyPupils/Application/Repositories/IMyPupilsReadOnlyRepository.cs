using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.Repositories;
public interface IMyPupilsReadOnlyRepository
{
    Task<Domain.AggregateRoot.MyPupils> GetMyPupils(MyPupilsId id);
    Task<Domain.AggregateRoot.MyPupils?> GetMyPupilsOrDefaultAsync(MyPupilsId id);
}
