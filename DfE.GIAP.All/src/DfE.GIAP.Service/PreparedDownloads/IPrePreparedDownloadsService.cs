using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Models.PrePreparedDownloads;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DfE.GIAP.Service.PreparedDownloads
{
    public interface IPrePreparedDownloadsService
    {
        Task<IList<PrePreparedDownloads>> GetPrePreparedDownloadsList(AzureFunctionHeaderDetails azureFunctionHeaderDetails, bool isOrganisationLocalAuthority = false, bool isOrganisationMultiAcademyTrust = false,  bool isOrganisationEstablishment = false, bool isOrganisationSingleAcademyTrust = false, string getEstablishmentNumber = "", string uniqueIdentifier = "", string getLocalAuthorityNumber = "", string uniqueReferenceNumber = "");
        Task PrePreparedDownloadsFileAsync(string fileName, Stream stream, AzureFunctionHeaderDetails azureFunctionHeaderDetails, bool IsOrganisationLocalAuthority = false, bool IsOrganisationMultiAcademyTrust = false,  bool IsOrganisationEstablishment = false, bool IsOrganisationSingleAcademyTrust = false, string GetEstablishmentNumber = "", string UniqueIdentifier = "", string GetLocalAuthorityNumber = "", string UniqueReferenceNumber = "");
        Task<string> BuildFolderPath(bool IsOrgnisationLocalAuthority = false, bool IsOrganisationMultiAcademyTrust = false, bool IsOrganisationEstablishment = false, bool IsOrganisationSingleAcademyTrust = false, string GetEstablishmentNumber = "", string UniqueIdentifier = "", string GetLocalAuthorityNumber = "", string UniqueReferenceNumber = "");
    }
}
