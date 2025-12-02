using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Domain;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;
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

        MyPupilsId id = new(request.UserId);

        MyPupilsAggregate myPupils = await _readRepository.GetMyPupils(id);

        myPupils.AddPupils(
            UniquePupilNumbers.Create(
                request.UniquePupilNumbers.ToUniquePupilNumbers()));

        await _writeRepository.SaveMyPupilsAsync(myPupils);
    }
}
