namespace DfE.GIAP.Core.MyPupils.Application.Repositories;

public interface IMyPupilsWriteOnlyRepository
{
    Task SaveMyPupilsAsync(MyPupilsAggregate myPupils);
}
