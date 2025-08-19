using DfE.GIAP.Core.Common.Infrastructure.BlobStorage;

namespace DfE.GIAP.Core.PrePreparedDownloads.Application.UseCases.GetPrePreparedFiles;
public record GetPrePreparedFilesResponse(IEnumerable<BlobItemInfo> BlobStorageItems);
