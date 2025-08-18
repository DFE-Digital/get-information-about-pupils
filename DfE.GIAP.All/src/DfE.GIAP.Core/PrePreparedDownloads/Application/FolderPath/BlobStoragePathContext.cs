using DfE.GIAP.Core.PrePreparedDownloads.Application.Enums;
namespace DfE.GIAP.Core.PrePreparedDownloads.Application.FolderPath;

public class BlobStoragePathContext
{
    public OrganisationType OrganisationType { get; set; } = OrganisationType.Unknown;
    public string UniqueIdentifier { get; set; } = string.Empty;
    public string LocalAuthorityNumber { get; set; } = string.Empty;
    public string UniqueReferenceNumber { get; set; } = string.Empty;
}
