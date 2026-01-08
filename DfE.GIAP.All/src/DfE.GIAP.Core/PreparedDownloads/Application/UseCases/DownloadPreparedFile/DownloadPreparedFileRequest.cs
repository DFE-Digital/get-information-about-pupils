using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.PreparedDownloads.Application.FolderPath;

namespace DfE.GIAP.Core.PreparedDownloads.Application.UseCases.DownloadPreparedFile;

public record DownloadPreparedFileRequest(string FileName, BlobStoragePathContext PathContext) : IUseCaseRequest<DownloadPreparedFileResponse>;
