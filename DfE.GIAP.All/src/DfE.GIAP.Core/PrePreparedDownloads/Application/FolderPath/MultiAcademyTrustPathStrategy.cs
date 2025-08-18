using DfE.GIAP.Core.PrePreparedDownloads.Application.Enums;

namespace DfE.GIAP.Core.PrePreparedDownloads.Application.FolderPath;
public class MultiAcademyTrustPathStrategy : IBlobStoragePathStrategy
{
    public bool CanHandle(OrganisationType type) => type == OrganisationType.MultiAcademyTrust;

    public string ResolvePath(BlobStoragePathContext context)
        => $"MAT/{context.UniqueIdentifier}/";
}
