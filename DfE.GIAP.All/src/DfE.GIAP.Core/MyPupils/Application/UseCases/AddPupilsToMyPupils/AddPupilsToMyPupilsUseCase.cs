using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.Users.Application.Models;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;
internal sealed class AddPupilsToMyPupilsUseCase : IUseCaseRequestOnly<AddPupilsToMyPupilsRequest>
{
    private readonly IMyPupilsReadOnlyRepository _readRepository;
    private readonly IMyPupilsWriteOnlyRepository _writeRepository;
    private readonly IMapper<IEnumerable<string>, UniquePupilNumbers> _mapToUniquePupilNumbers;

    public AddPupilsToMyPupilsUseCase(
        IMyPupilsReadOnlyRepository readRepository,
        IMyPupilsWriteOnlyRepository writeRepository,
        IMapper<IEnumerable<string>, UniquePupilNumbers> mapToUniquePupilNumbers)
    {
        ArgumentNullException.ThrowIfNull(readRepository);
        _readRepository = readRepository;

        ArgumentNullException.ThrowIfNull(writeRepository);
        _writeRepository = writeRepository;

        ArgumentNullException.ThrowIfNull(mapToUniquePupilNumbers);
        _mapToUniquePupilNumbers = mapToUniquePupilNumbers;
    }

    public async Task HandleRequestAsync(AddPupilsToMyPupilsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        UserId userId = new(request.UserId);

        MyPupilsId id = new(userId);

        MyPupilsAggregate myPupils = await _readRepository.GetMyPupils(id);

        myPupils.AddPupils(
            addPupilNumbers: _mapToUniquePupilNumbers.Map(request.UniquePupilNumbers));

        await _writeRepository.SaveMyPupilsAsync(myPupils);
    }
}
