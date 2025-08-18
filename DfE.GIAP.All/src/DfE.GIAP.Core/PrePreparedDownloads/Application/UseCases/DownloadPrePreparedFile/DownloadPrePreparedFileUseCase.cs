using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.Infrastructure.BlobStorage;
using DfE.GIAP.Core.PrePreparedDownloads.Application.FolderPath;

namespace DfE.GIAP.Core.PrePreparedDownloads.Application.UseCases.DownloadPrePreparedFile;
internal class DownloadPrePreparedFileUseCase : IUseCase<DownloadPrePreparedFileRequest, DownloadPrePreparedFileResponse>
{
    private readonly IBlobStorageProvider _blobStorageProvider;
    private readonly IBlobStoragePathBuilder _folderPathBuilder;

    public DownloadPrePreparedFileUseCase(
        IBlobStorageProvider blobStorageProvider,
        IBlobStoragePathBuilder folderPathBuilder)
    {
        ArgumentNullException.ThrowIfNull(blobStorageProvider);
        _blobStorageProvider = blobStorageProvider;

        ArgumentNullException.ThrowIfNull(folderPathBuilder);
        _folderPathBuilder = folderPathBuilder;
    }

    public async Task<DownloadPrePreparedFileResponse> HandleRequestAsync(DownloadPrePreparedFileRequest request)
    {
        string directory = _folderPathBuilder.BuildPath(request.FolderContext);
        Stream stream = await _blobStorageProvider.DownloadAsync("giapdownloads", $"{directory}{request.FileName}");

        return new DownloadPrePreparedFileResponse(stream, request.FileName);
    }
}
