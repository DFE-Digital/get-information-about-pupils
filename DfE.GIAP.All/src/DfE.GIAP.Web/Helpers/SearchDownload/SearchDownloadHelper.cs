using DfE.GIAP.Common.Constants;
using DfE.GIAP.Domain.Models.Common;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Helpers.SearchDownload;

public static class SearchDownloadHelper
{
    /// <summary>
    /// Prepares a file for download by returning an IActionResult.
    /// </summary>
    public static IActionResult DownloadFile(ReturnFile downloadFile)
    {
        string contentType = 
            downloadFile.FileType == FileType.ZipFile
                ? FileType.ZipContentType
                : $"text/{downloadFile.FileType}";

        return new FileContentResult(downloadFile.Bytes, contentType)
        {
            FileDownloadName = downloadFile.FileName
        };
    }
}
