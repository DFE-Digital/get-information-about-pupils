using DfE.GIAP.Common.Enums;
using DfE.GIAP.Domain.Models.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DfE.GIAP.Service.Download
{
    public interface IUlnDownloadService
    {
        Task<ReturnFile> GetUlnCSVFile(string[] selectedPupils, string[] selectedDownloadOptions, bool confirmationGiven, AzureFunctionHeaderDetails azureFunctionHeaderDetails, ReturnRoute returnRoute);
        Task<IEnumerable<DownloadUlnDataType>> CheckForNoDataAvailable(string[] selectedPupils, string[] selectedDownloadOptions, AzureFunctionHeaderDetails azureFunctionHeaderDetails);
    }
}
