using DfE.GIAP.Core.Common.Infrastructure.BlobStorage;

namespace DfE.GIAP.Core.PreparedDownloads.Application.UseCases.GetPreparedFiles;
public record GetPreparedFilesResponse(IEnumerable<BlobItemMetadata> BlobStorageItems);
