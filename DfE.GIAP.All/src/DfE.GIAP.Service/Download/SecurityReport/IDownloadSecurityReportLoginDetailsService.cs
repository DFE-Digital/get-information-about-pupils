using System.Threading.Tasks;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Domain.Models.Common;

namespace DfE.GIAP.Service.Download.SecurityReport
{
    public interface IDownloadSecurityReportLoginDetailsService
    {
        Task<ReturnFile> GetSecurityReportLoginDetails(string searchParameter, SecurityReportSearchType searchType, AzureFunctionHeaderDetails azureFunctionHeaderDetails);
    }
}