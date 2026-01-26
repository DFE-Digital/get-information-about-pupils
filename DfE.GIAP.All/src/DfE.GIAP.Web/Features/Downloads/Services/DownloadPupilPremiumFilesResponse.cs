using DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilDatasets;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Features.Downloads.Services;

public record DownloadPupilPremiumFilesResponse
{
    private readonly DownloadPupilDataResponse _response;

    public DownloadPupilPremiumFilesResponse(DownloadPupilDataResponse response)
    {
        _response = response;
    }

    // TODO should back behind DownloadPupilDataResponse?
    public bool HasData => _response is not null &&
        _response.FileContents is not null &&
            _response.FileContents.Length > 0 &&
                 !string.IsNullOrWhiteSpace(_response.FileName) &&
                    !string.IsNullOrWhiteSpace(_response.ContentType);

    public IActionResult GetResult()
    {
        if (!HasData)
        {
            return new EmptyResult();
        }

        return new FileStreamResult(
            fileStream: new MemoryStream(_response.FileContents), // TODO replace with a Stream so bytes do not have to be read in mem
            contentType: _response.ContentType)
        {
            FileDownloadName = _response.FileName
        };
    }
}
