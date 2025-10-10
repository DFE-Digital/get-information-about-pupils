namespace DfE.GIAP.Core.MyPupils.Application.Repositories;

public interface IMyPupilsWriteOnlyRepository
{
    Task SaveMyPupilsAsync(Domain.AggregateRoot.MyPupils myPupils);
}
