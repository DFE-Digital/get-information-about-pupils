using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Common.Enums;

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
