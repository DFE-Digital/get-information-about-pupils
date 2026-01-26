namespace DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
public record DeletePupilsFromMyPupilsRequest(
    string UserId,
    IEnumerable<string> DeletePupilUpns) : IUseCaseRequest;
