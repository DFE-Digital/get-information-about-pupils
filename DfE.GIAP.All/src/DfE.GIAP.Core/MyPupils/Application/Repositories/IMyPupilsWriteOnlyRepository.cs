using DfE.GIAP.Core.MyPupils.Domain;

namespace DfE.GIAP.Core.MyPupils.Application.Repositories;

public interface IMyPupilsWriteOnlyRepository
{
    Task SaveMyPupilsAsync(MyPupilsAggregate myPupils);
}
