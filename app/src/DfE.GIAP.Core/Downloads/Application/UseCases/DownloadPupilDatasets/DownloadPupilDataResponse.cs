namespace DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilDatasets;

public record DownloadPupilDataResponse(
    byte[]? FileContents = null,
    string? FileName = null,
    string? ContentType = null);
