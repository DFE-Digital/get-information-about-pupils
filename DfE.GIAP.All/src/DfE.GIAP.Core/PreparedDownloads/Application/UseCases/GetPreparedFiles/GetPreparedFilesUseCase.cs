using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.Infrastructure.BlobStorage;

namespace DfE.GIAP.Core.PreparedDownloads.Application.UseCases.GetPreparedFiles;
public class GetPreparedFilesUseCase : IUseCase<GetPreparedFilesRequest, GetPreparedFilesResponse>
{
    private readonly IBlobStorageProvider _blobStorageProvider;

    public GetPreparedFilesUseCase(IBlobStorageProvider blobStorageProvider)
    {
        ArgumentNullException.ThrowIfNull(blobStorageProvider);
        _blobStorageProvider = blobStorageProvider;
    }

    public async Task<GetPreparedFilesResponse> HandleRequestAsync(GetPreparedFilesRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string directory = request.PathContext.ResolvePath();
        IEnumerable<BlobItemMetadata> response = await _blobStorageProvider.ListBlobsWithMetadataAsync("giapdownloads", directory);

        return new GetPreparedFilesResponse(response);
    }
}
