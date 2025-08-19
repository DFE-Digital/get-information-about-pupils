using DfE.GIAP.Core.PrePreparedDownloads.Application.Enums;

namespace DfE.GIAP.Core.PrePreparedDownloads.Application.FolderPath;

public class AzureBlobStoragePathResolver : IBlobStoragePathResolver
{
    public string ResolvePath(BlobStoragePathContext context)
    {
        return context.OrganisationScope switch
        {
            OrganisationScope.Establishment => $"School/{context.UniqueReferenceNumber}/",
            OrganisationScope.LocalAuthority => $"LA/{context.LocalAuthorityNumber}/",
            OrganisationScope.MultiAcademyTrust => $"MAT/{context.UniqueIdentifier}/",
            OrganisationScope.SingleAcademyTrust => $"SAT/{context.UniqueIdentifier}/",
            OrganisationScope.AllUsers => "AllUsers/Metadata/",
            _ => throw new NotImplementedException()
        };
    }
}
