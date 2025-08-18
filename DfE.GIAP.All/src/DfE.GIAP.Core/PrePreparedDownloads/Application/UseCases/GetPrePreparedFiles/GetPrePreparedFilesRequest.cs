using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.PrePreparedDownloads.Application.FolderPath;

namespace DfE.GIAP.Core.PrePreparedDownloads.Application.UseCases.GetPrePreparedFiles;
public record GetPrePreparedFilesRequest(BlobStoragePathContext FolderContext) : IUseCaseRequest<GetPrePreparedFilesResponse>
{
}
