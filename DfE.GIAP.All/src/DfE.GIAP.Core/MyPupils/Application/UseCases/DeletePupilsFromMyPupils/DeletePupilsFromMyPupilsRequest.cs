using DfE.GIAP.Core.Common.Application;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
public record DeletePupilsFromMyPupilsRequest(
    string UserId,
    IEnumerable<string> DeletePupilUpns,
    bool DeleteAll) : IUseCaseRequest;
