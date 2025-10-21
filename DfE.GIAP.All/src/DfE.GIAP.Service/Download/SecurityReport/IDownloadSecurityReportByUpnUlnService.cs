using DfE.GIAP.Domain.Models.Common;

namespace DfE.GIAP.Service.Download.SecurityReport;

public interface IDownloadSecurityReportByUpnUlnService
{
    Task<ReturnFile> GetSecurityReportByUpn(string upn,AzureFunctionHeaderDetails azureFunctionHeaderDetails);
    Task<ReturnFile> GetSecurityReportByUln(string uln, AzureFunctionHeaderDetails azureFunctionHeaderDetails);

}
