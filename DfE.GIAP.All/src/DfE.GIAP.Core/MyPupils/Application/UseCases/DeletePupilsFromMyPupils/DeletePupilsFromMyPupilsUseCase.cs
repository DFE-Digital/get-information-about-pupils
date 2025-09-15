using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
internal sealed class DeletePupilsFromMyPupilsUseCase : IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>
{
    private readonly IMyPupilsReadOnlyRepository _myPupilsReadOnlyRepository;
    private readonly IMyPupilsWriteOnlyRepository _myPupilsWriteOnlyRepository;

    public DeletePupilsFromMyPupilsUseCase(
        IMyPupilsReadOnlyRepository myPupilsReadOnlyRepository,
        IMyPupilsWriteOnlyRepository myPupilsWriteOnlyRepository)
    {
        ArgumentNullException.ThrowIfNull(myPupilsReadOnlyRepository);
        _myPupilsReadOnlyRepository = myPupilsReadOnlyRepository;

        ArgumentNullException.ThrowIfNull(myPupilsWriteOnlyRepository);
        _myPupilsWriteOnlyRepository = myPupilsWriteOnlyRepository;
    }

    public async Task HandleRequestAsync(DeletePupilsFromMyPupilsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        Domain.AggregateRoot.MyPupils? myPupils = await _myPupilsReadOnlyRepository.GetMyPupilsOrDefaultAsync(request.UserId);

        if(myPupils is null)
        {
            return; // nothing to delete
        }

        if (request.DeleteAll)
        {
            myPupils.DeleteAll();
            await _myPupilsWriteOnlyRepository.SaveMyPupilsAsync(request.UserId, myPupils);
            return;
        }

        myPupils.DeletePupils(
            UniquePupilNumbers.Create(
                uniquePupilNumbers: request.DeletePupilUpns));

        await _myPupilsWriteOnlyRepository.SaveMyPupilsAsync(
            request.UserId,
            myPupils,
            request.CancellationToken);
    }
}
