using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.Infrastructure.BlobStorage;
using DfE.GIAP.Core.PrePreparedDownloads.Application.FolderPath;
using DfE.GIAP.Core.PrePreparedDownloads.Application.UseCases.DownloadPrePreparedFile;

namespace DfE.GIAP.Core.PrePreparedDownloads.Application.UseCases.GetPrePreparedFiles;
public class GetPrePreparedFilesUseCase : IUseCase<GetPrePreparedFilesRequest, GetPrePreparedFilesResponse>
{
    private readonly IBlobStorageProvider _blobStorageProvider;
    private readonly IBlobStoragePathResolver _folderPathBuilder;

    public GetPrePreparedFilesUseCase(
        IBlobStorageProvider blobStorageProvider,
        IBlobStoragePathResolver folderPathBuilder)
    {
        ArgumentNullException.ThrowIfNull(blobStorageProvider);
        _blobStorageProvider = blobStorageProvider;

        ArgumentNullException.ThrowIfNull(folderPathBuilder);
        _folderPathBuilder = folderPathBuilder;
    }

    public async Task<GetPrePreparedFilesResponse> HandleRequestAsync(GetPrePreparedFilesRequest request)
    {
        string directory = _folderPathBuilder.ResolvePath(request.PathContext);
        IEnumerable<BlobItemInfo> response = await _blobStorageProvider.ListBlobsWithMetadataAsync("giapdownloads", directory);

        return new GetPrePreparedFilesResponse(response);
    }
}
