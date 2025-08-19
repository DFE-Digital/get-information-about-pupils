namespace DfE.GIAP.Core.PrePreparedDownloads.Application.FolderPath;

public interface IBlobStoragePathResolver
{
    string ResolvePath(BlobStoragePathContext context);
}
