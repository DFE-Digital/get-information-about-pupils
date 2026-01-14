using System.Net.Mime;

namespace DfE.GIAP.Core.PreparedDownloads.Application.UseCases.DownloadPreparedFile;

public record DownloadPreparedFileResponse(Stream FileStream, string FileName, string ContentType = MediaTypeNames.Text.Plain);
