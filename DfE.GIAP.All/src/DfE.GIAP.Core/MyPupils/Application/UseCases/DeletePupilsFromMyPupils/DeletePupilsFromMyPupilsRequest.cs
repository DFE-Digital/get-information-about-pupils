namespace DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
public record DeletePupilsFromMyPupilsRequest : IUseCaseRequest
{
    public DeletePupilsFromMyPupilsRequest(string userId, IEnumerable<string> deletePupilUpns)
    {
        UserId = userId ?? string.Empty;

        DeletePupilUpns = (deletePupilUpns ?? []).ToList().AsReadOnly();
    }

    public string UserId { get; }
    public IReadOnlyList<string> DeletePupilUpns { get; }
}
