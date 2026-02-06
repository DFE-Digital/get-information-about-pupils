namespace DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilCtf;

public record DownloadPupilCtfResponse(
    byte[]? FileContents = null,
    string? FileName = null,
    string? ContentType = null);
