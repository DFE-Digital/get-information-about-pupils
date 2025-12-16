using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.Users.Application.Models;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
internal sealed class DeletePupilsFromMyPupilsUseCase : IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>
{
    private readonly IMyPupilsReadOnlyRepository _myPupilsReadOnlyRepository;
    private readonly IMyPupilsWriteOnlyRepository _myPupilsWriteOnlyRepository;
    private readonly IMapper<IEnumerable<string>, UniquePupilNumbers> _mapToUniquePupilNumbers;

    public DeletePupilsFromMyPupilsUseCase(
        IMyPupilsReadOnlyRepository myPupilsReadOnlyRepository,
        IMyPupilsWriteOnlyRepository myPupilsWriteOnlyRepository,
        IMapper<IEnumerable<string>, UniquePupilNumbers> mapToUniquePupilNumbers)
    {
        ArgumentNullException.ThrowIfNull(myPupilsReadOnlyRepository);
        _myPupilsReadOnlyRepository = myPupilsReadOnlyRepository;

        ArgumentNullException.ThrowIfNull(myPupilsWriteOnlyRepository);
        _myPupilsWriteOnlyRepository = myPupilsWriteOnlyRepository;

        ArgumentNullException.ThrowIfNull(mapToUniquePupilNumbers);
        _mapToUniquePupilNumbers = mapToUniquePupilNumbers;
    }

    public async Task HandleRequestAsync(DeletePupilsFromMyPupilsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        UserId userId = new(request.UserId);

        MyPupilsId id = new(userId);

        MyPupilsAggregate? myPupils = await _myPupilsReadOnlyRepository.GetMyPupilsOrDefaultAsync(id);

        if (myPupils is null)
        {
            return; // nothing to delete
        }

        myPupils.DeletePupils(
            _mapToUniquePupilNumbers.Map(request.DeletePupilUpns));

        await _myPupilsWriteOnlyRepository.SaveMyPupilsAsync(myPupils);
    }
}
