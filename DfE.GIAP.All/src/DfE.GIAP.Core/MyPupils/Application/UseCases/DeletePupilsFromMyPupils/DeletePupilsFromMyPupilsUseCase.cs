using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Domain;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.Users.Application.Models;

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
        MyPupilsId id = new(request.UserId);

        MyPupilsAggregate? myPupils = await _myPupilsReadOnlyRepository.GetMyPupilsOrDefaultAsync(id);

        if (myPupils is null)
        {
            return; // nothing to delete
        }

        myPupils.DeletePupils(
            UniquePupilNumbers.Create(
                uniquePupilNumbers: request.DeletePupilUpns.ToUniquePupilNumbers()));

        await _myPupilsWriteOnlyRepository.SaveMyPupilsAsync(myPupils);
    }
}
