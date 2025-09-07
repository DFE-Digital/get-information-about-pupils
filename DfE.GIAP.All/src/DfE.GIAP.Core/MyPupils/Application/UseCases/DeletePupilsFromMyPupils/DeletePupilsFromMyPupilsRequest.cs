using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
public record DeletePupilsFromMyPupilsRequest(
    string UserId,
    IEnumerable<UniquePupilNumber> DeletePupilUpns,
    bool DeleteAll) : IUseCaseRequest;
