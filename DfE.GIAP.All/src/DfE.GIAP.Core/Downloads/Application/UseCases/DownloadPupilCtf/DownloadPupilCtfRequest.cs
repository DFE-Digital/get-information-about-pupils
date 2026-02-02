namespace DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilCtf;

public record DownloadPupilCtfRequest(IEnumerable<string> SelectedPupils) : IUseCaseRequest<DownloadPupilCtfResponse>;

public record DownloadPupilCtfResponse(
    byte[]? FileContents = null,
    string? FileName = null,
    string? ContentType = null);

public class DownloadPupilCtfUseCase : IUseCase<DownloadPupilCtfRequest, DownloadPupilCtfResponse>
{
    public Task<DownloadPupilCtfResponse> HandleRequestAsync(DownloadPupilCtfRequest request)
    {
        throw new NotImplementedException();
    }
}
