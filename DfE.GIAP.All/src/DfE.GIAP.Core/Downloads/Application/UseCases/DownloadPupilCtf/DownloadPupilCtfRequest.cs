namespace DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilCtf;

public record DownloadPupilCtfRequest(IEnumerable<string> SelectedPupils) : IUseCaseRequest<DownloadPupilCtfResponse>;

public record DownloadPupilCtfResponse(
    byte[]? FileContents = null,
    string? FileName = null,
    string? ContentType = null);

public class DownloadPupilCtfUseCase : IUseCase<DownloadPupilCtfRequest, DownloadPupilCtfResponse>
{
    private readonly IPupilCtfAggregator _pupilCtfAggregator;

    public DownloadPupilCtfUseCase(IPupilCtfAggregator pupilCtfAggregator)
    {
        ArgumentNullException.ThrowIfNull(pupilCtfAggregator);
        _pupilCtfAggregator = pupilCtfAggregator;
    }

    public async Task<DownloadPupilCtfResponse> HandleRequestAsync(DownloadPupilCtfRequest request)
    {
        PupilCtfAggregate test = await _pupilCtfAggregator.AggregateAsync(request.SelectedPupils);
        throw new NotImplementedException();
    }
}
