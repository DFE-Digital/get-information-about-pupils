using System.Threading.Tasks;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Domain.Models.Common;

namespace DfE.GIAP.Service.Download.SecurityReport
{
    public interface IDownloadSecurityReportDetailedSearchesService
    {
        Task<ReturnFile> GetSecurityReportDetailedSearches(string searchParameter, SecurityReportSearchType searchType, AzureFunctionHeaderDetails azureFunctionHeaderDetails, bool isFe);
    }
}