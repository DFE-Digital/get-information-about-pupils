namespace DfE.GIAP.Core.PreparedDownloads.Application.FolderPath;

public interface IBlobStoragePathResolver
{
    string ResolvePath(BlobStoragePathContext context);
}
