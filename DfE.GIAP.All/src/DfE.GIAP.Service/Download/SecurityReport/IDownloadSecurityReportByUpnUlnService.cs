using DfE.GIAP.Domain.Models.Common;
using System.Threading.Tasks;

namespace DfE.GIAP.Service.Download.SecurityReport
{
    public interface IDownloadSecurityReportByUpnUlnService
    {
        Task<ReturnFile> GetSecurityReportByUpn(string upn,AzureFunctionHeaderDetails azureFunctionHeaderDetails);
        Task<ReturnFile> GetSecurityReportByUln(string uln, AzureFunctionHeaderDetails azureFunctionHeaderDetails);

    }
}
