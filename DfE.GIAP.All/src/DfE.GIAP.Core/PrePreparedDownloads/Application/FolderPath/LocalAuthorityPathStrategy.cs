using DfE.GIAP.Core.PrePreparedDownloads.Application.Enums;

namespace DfE.GIAP.Core.PrePreparedDownloads.Application.FolderPath;
public class LocalAuthorityPathStrategy : IBlobStoragePathStrategy
{
    public bool CanHandle(OrganisationType type) => type == OrganisationType.LocalAuthority;

    public string ResolvePath(BlobStoragePathContext context)
        => $"LA/{context.LocalAuthorityNumber}/";
}
