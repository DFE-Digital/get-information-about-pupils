using DfE.GIAP.Core.PrePreparedDownloads.Application.Enums;

namespace DfE.GIAP.Core.PrePreparedDownloads.Application.FolderPath;
public class SingleAcademyTrustPathStrategy : IBlobStoragePathStrategy
{
    public bool CanHandle(OrganisationType type) => type == OrganisationType.SingleAcademyTrust;

    public string ResolvePath(BlobStoragePathContext context)
        => $"SAT/{context.UniqueIdentifier}/";
}
