using DfE.GIAP.Common.Enums;
using DfE.GIAP.Domain.Models.Common;

namespace DfE.GIAP.Web.Services.Download.CTF;

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
