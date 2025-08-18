using System.Net.Mime;

namespace DfE.GIAP.Core.PrePreparedDownloads.Application.UseCases.DownloadPrePreparedFile;
public record DownloadPrePreparedFileResponse(Stream FileStream, string FileName, string ContentType = MediaTypeNames.Text.Plain);
