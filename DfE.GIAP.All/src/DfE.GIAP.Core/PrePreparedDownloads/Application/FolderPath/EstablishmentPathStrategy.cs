using DfE.GIAP.Core.PrePreparedDownloads.Application.Enums;

namespace DfE.GIAP.Core.PrePreparedDownloads.Application.FolderPath;
public class EstablishmentPathStrategy : IBlobStoragePathStrategy
{
    public bool CanHandle(OrganisationType type) => type == OrganisationType.Establishment;

    public string ResolvePath(BlobStoragePathContext context)
        => $"School/{context.UniqueReferenceNumber}/";
}
