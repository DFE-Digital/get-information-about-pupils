namespace DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilDatasets;

public record DownloadPupilDataResponse(
    byte[] FileContents,
    string FileName,
    string ContentType);
