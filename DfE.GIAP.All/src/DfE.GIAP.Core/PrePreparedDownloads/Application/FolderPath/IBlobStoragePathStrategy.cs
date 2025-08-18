using DfE.GIAP.Core.PrePreparedDownloads.Application.Enums;

namespace DfE.GIAP.Core.PrePreparedDownloads.Application.FolderPath;
public interface IBlobStoragePathStrategy
{
    bool CanHandle(OrganisationType type);
    string ResolvePath(BlobStoragePathContext context);
}
