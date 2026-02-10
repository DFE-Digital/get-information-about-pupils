using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Common.Enums;

namespace DfE.GIAP.Web.Services.Download;

public interface IDownloadService
{
    Task<ReturnFile> GetCSVFile(string[] selectedPupils, string[] sortOrder, string[] selectedDownloadOptions, bool confirmationGiven, AzureFunctionHeaderDetails azureFunctionHeaderDetails, ReturnRoute returnRoute);
    Task<ReturnFile> GetTABFile(string[] selectedPupils, string[] sortOrder, string[] selectedDownloadOptions, bool confirmationGiven, AzureFunctionHeaderDetails azureFunctionHeaderDetails, ReturnRoute returnRoute);
    Task<IEnumerable<CheckDownloadDataType>> CheckForNoDataAvailable(string[] selectedPupils, string[] sortOrder, string[] selectedDownloadOptions, AzureFunctionHeaderDetails azureFunctionHeaderDetails);
}
