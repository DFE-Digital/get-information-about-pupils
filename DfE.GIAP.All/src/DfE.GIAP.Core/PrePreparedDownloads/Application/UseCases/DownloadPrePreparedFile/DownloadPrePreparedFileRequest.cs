using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.PrePreparedDownloads.Application.FolderPath;

namespace DfE.GIAP.Core.PrePreparedDownloads.Application.UseCases.DownloadPrePreparedFile;

public record DownloadPrePreparedFileRequest(string FileName, BlobStoragePathContext PathContext) : IUseCaseRequest<DownloadPrePreparedFileResponse>;
