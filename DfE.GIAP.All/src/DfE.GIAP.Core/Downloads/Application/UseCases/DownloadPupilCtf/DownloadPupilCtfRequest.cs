namespace DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilCtf;

public record DownloadPupilCtfRequest(IEnumerable<string> SelectedPupils) : IUseCaseRequest<DownloadPupilCtfResponse>;
