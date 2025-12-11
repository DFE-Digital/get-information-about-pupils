using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.Users.Application.Models;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.DeleteAllPupilsFromMyPupils;
internal sealed class DeleteAllMyPupilsUseCase : IUseCaseRequestOnly<DeleteAllMyPupilsRequest>
{
    private readonly IMyPupilsReadOnlyRepository _readOnlyRepository;
    private readonly IMyPupilsWriteOnlyRepository _writeOnlyRepository;

    public DeleteAllMyPupilsUseCase(
        IMyPupilsReadOnlyRepository readOnlyRepository,
        IMyPupilsWriteOnlyRepository writeOnlyRepository)
    {
        ArgumentNullException.ThrowIfNull(readOnlyRepository);
        _readOnlyRepository = readOnlyRepository;

        ArgumentNullException.ThrowIfNull(writeOnlyRepository);
        _writeOnlyRepository = writeOnlyRepository;
    }

    public async Task HandleRequestAsync(DeleteAllMyPupilsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        UserId userId = new(request.UserId);

        MyPupilsId id = new(userId);

        MyPupilsAggregate? myPupils = await _readOnlyRepository.GetMyPupilsOrDefaultAsync(id);

        if (myPupils is null)
        {
            return; // nothing to delete
        }

        myPupils.DeleteAll();

        await _writeOnlyRepository.SaveMyPupilsAsync(myPupils);
    }
}
