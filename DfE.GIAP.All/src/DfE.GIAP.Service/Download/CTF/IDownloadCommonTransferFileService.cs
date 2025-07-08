using DfE.GIAP.Domain.Models.Common;
using System.Threading.Tasks;
using DfE.GIAP.Common.Enums;

namespace DfE.GIAP.Service.Download.CTF
{
    public interface IDownloadCommonTransferFileService
    {
        Task<ReturnFile> GetCommonTransferFile(string[] upns,
                                               string[] sortOrder,
                                               string localAuthorityNumber,
                                               string establishmentNumber,
                                               bool isOrganisationEstablishment,
                                               AzureFunctionHeaderDetails azureFunctionHeaderDetails,
                                               ReturnRoute returnRoute);
    }
}