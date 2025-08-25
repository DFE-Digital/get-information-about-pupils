using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.Infrastructure.BlobStorage;
using DfE.GIAP.Core.PreparedDownloads.Application.FolderPath;

namespace DfE.GIAP.Core.PreparedDownloads.Application.UseCases.DownloadPreparedFile;
internal class DownloadPreparedFileUseCase : IUseCase<DownloadPreparedFileRequest, DownloadPreparedFileResponse>
{
    private readonly IBlobStorageProvider _blobStorageProvider;
    private readonly IBlobStoragePathResolver _folderPathBuilder;

    public DownloadPreparedFileUseCase(
        IBlobStorageProvider blobStorageProvider,
        IBlobStoragePathResolver folderPathBuilder)
    {
        ArgumentNullException.ThrowIfNull(blobStorageProvider);
        _blobStorageProvider = blobStorageProvider;

        ArgumentNullException.ThrowIfNull(folderPathBuilder);
        _folderPathBuilder = folderPathBuilder;
    }

    public async Task<DownloadPreparedFileResponse> HandleRequestAsync(DownloadPreparedFileRequest request)
    {
        string directory = _folderPathBuilder.ResolvePath(request.PathContext);
        Stream stream = await _blobStorageProvider.DownloadAsync("giapdownloads", $"{directory}{request.FileName}");

        return new DownloadPreparedFileResponse(stream, request.FileName);
    }
}
