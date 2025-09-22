using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting.Logging;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Models;
using DfE.GIAP.Core.Common.Infrastructure.BlobStorage;

namespace DfE.GIAP.Core.PreparedDownloads.Application.UseCases.DownloadPreparedFile;
internal class DownloadPreparedFileUseCase : IUseCase<DownloadPreparedFileRequest, DownloadPreparedFileResponse>
{
    private readonly IBlobStorageProvider _blobStorageProvider;
    private readonly ILoggerService _logger;

    public DownloadPreparedFileUseCase(IBlobStorageProvider blobStorageProvider, ILoggerService logger)
    {
        ArgumentNullException.ThrowIfNull(blobStorageProvider);
        _blobStorageProvider = blobStorageProvider;

        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public async Task<DownloadPreparedFileResponse> HandleRequestAsync(DownloadPreparedFileRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        _logger.LogTrace(
            level: LogLevel.Information,
            message: "Debug Log",
            source: "DownloadPreparedFileUseCase",
            category: "Prepared_Files");

        string directory = request.PathContext.ResolvePath();
        Stream stream = await _blobStorageProvider.DownloadBlobAsStreamAsync("giapdownloads", $"{directory}{request.FileName}");

        return new DownloadPreparedFileResponse(stream, request.FileName);
    }
}
