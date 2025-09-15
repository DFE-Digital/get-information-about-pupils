using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;

// TODO consume in both Text and Number for NPD and PP
public sealed class AddPupilsToMyPupilsUseCase : IUseCaseRequestOnly<AddPupilsToMyPupilsRequest>
{
    private readonly IMyPupilsReadOnlyRepository _readRepository;
    private readonly IMyPupilsWriteOnlyRepository _writeRepository;

    public AddPupilsToMyPupilsUseCase(
        IMyPupilsReadOnlyRepository readRepository,
        IMyPupilsWriteOnlyRepository writeRepository)
    {
        ArgumentNullException.ThrowIfNull(readRepository);
        _readRepository = readRepository;

        ArgumentNullException.ThrowIfNull(writeRepository);
        _writeRepository = writeRepository;
    }

    public async Task HandleRequestAsync(AddPupilsToMyPupilsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        Domain.AggregateRoot.MyPupils myPupils = await _readRepository.GetMyPupils(request.UserId, request.CancellationToken);

        myPupils.Add(
            UniquePupilNumbers.Create(request.Pupils));

        await _writeRepository.SaveMyPupilsAsync(request.UserId, myPupils, request.CancellationToken);
    }
}
