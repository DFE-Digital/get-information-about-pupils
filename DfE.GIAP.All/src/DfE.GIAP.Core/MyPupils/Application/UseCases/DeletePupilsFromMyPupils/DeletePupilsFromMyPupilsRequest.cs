using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.Users.Application;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
public record DeletePupilsFromMyPupilsRequest(
    UserId UserId,
    IEnumerable<UniquePupilNumber> DeletePupilUpns,
    bool DeleteAll,
    CancellationToken CancellationToken) : IUseCaseRequest;
