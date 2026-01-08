using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.Infrastructure.BlobStorage;

namespace DfE.GIAP.Core.PreparedDownloads.Application.UseCases.DownloadPreparedFile;

internal class DownloadPreparedFileUseCase : IUseCase<DownloadPreparedFileRequest, DownloadPreparedFileResponse>
{
    private readonly IBlobStorageProvider _blobStorageProvider;

    public DownloadPreparedFileUseCase(IBlobStorageProvider blobStorageProvider)
    {
        ArgumentNullException.ThrowIfNull(blobStorageProvider);
        _blobStorageProvider = blobStorageProvider;
    }

    public async Task<DownloadPreparedFileResponse> HandleRequestAsync(DownloadPreparedFileRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        string directory = request.PathContext.ResolvePath();
        Stream stream = await _blobStorageProvider.DownloadBlobAsStreamAsync("giapdownloads", $"{directory}{request.FileName}");

        return new DownloadPreparedFileResponse(stream, request.FileName);
    }
}
