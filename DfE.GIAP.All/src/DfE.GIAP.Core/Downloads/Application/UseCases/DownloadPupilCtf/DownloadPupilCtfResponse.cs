namespace DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilCtf;

public record DownloadPupilCtfResponse(
    Stream FileStream,
    string FileName,
    string ContentType);

