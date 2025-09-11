using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.PreparedDownloads.Application.FolderPath;

namespace DfE.GIAP.Core.PreparedDownloads.Application.UseCases.GetPreparedFiles;
public record GetPreparedFilesRequest(BlobStoragePathContext PathContext) : IUseCaseRequest<GetPreparedFilesResponse>;
