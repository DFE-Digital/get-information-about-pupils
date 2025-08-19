using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.Infrastructure.BlobStorage;
using DfE.GIAP.Core.PreparedDownloads.Application.FolderPath;

namespace DfE.GIAP.Core.PreparedDownloads.Application.UseCases.GetPreparedFiles;
public class GetPreparedFilesUseCase : IUseCase<GetPreparedFilesRequest, GetPreparedFilesResponse>
{
    private readonly IBlobStorageProvider _blobStorageProvider;
    private readonly IBlobStoragePathResolver _folderPathBuilder;

    public GetPreparedFilesUseCase(
        IBlobStorageProvider blobStorageProvider,
        IBlobStoragePathResolver folderPathBuilder)
    {
        ArgumentNullException.ThrowIfNull(blobStorageProvider);
        _blobStorageProvider = blobStorageProvider;

        ArgumentNullException.ThrowIfNull(folderPathBuilder);
        _folderPathBuilder = folderPathBuilder;
    }

    public async Task<GetPreparedFilesResponse> HandleRequestAsync(GetPreparedFilesRequest request)
    {
        string directory = _folderPathBuilder.ResolvePath(request.PathContext);
        IEnumerable<BlobItemInfo> response = await _blobStorageProvider.ListBlobsWithMetadataAsync("giapdownloads", directory);

        return new GetPreparedFilesResponse(response);
    }
}
