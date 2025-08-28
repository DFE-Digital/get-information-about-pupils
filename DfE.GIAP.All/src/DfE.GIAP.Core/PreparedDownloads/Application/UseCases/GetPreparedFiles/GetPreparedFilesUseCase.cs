using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.Infrastructure.BlobStorage;

namespace DfE.GIAP.Core.PreparedDownloads.Application.UseCases.GetPreparedFiles;
public class GetPreparedFilesUseCase : IUseCase<GetPreparedFilesRequest, GetPreparedFilesResponse>
{
    private readonly IBlobStorageService _blobStorageProvider;

    public GetPreparedFilesUseCase(IBlobStorageService blobStorageProvider)
    {
        ArgumentNullException.ThrowIfNull(blobStorageProvider);
        _blobStorageProvider = blobStorageProvider;
    }

    public async Task<GetPreparedFilesResponse> HandleRequestAsync(GetPreparedFilesRequest request)
    {
        string directory = request.PathContext.ResolvePath();
        IEnumerable<BlobItemMetadata> response = await _blobStorageProvider.ListBlobsWithMetadataAsync("giapdownloads", directory);

        return new GetPreparedFilesResponse(response);
    }
}
