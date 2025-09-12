using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting.Logging;
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
        _logger.LogTrace(LogLevel.Verbose, "Verbose Log");
        _logger.LogTrace(LogLevel.Debug, "Debug Log");
        _logger.LogTrace(LogLevel.Information, "Information Log");
        _logger.LogTrace(LogLevel.Warning, "Warning Log");
        _logger.LogTrace(LogLevel.Error, "Error Log");
        _logger.LogTrace(LogLevel.Critical, "Critical Log");

        _logger.LogBusinessEvent("Business Event Log");

        string directory = request.PathContext.ResolvePath();
        Stream stream = await _blobStorageProvider.DownloadBlobAsStreamAsync("giapdownloads", $"{directory}{request.FileName}");

        return new DownloadPreparedFileResponse(stream, request.FileName);
    }
}
