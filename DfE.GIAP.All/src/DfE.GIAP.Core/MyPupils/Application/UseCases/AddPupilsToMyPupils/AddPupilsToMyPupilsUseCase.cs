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

        Repositories.MyPupils? current = await _readRepository.GetMyPupilsOrDefaultAsync(request.UserId, request.CancellationToken);

        UniquePupilNumbers currentMyPupils = current?.Pupils ?? UniquePupilNumbers.Create(uniquePupilNumbers: []);

        currentMyPupils.Add(request.Pupils);

        await _writeRepository.SaveMyPupilsAsync(request.UserId, currentMyPupils, request.CancellationToken);
    }
}
