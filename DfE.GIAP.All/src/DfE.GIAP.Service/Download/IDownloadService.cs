using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Core.Models.Glossary;
using DfE.GIAP.Common.Enums;

namespace DfE.GIAP.Service.Download;

public interface IDownloadService
{
    Task<ReturnFile> GetCSVFile(string[] selectedPupils, string[] sortOrder, string[] selectedDownloadOptions, bool confirmationGiven, AzureFunctionHeaderDetails azureFunctionHeaderDetails, ReturnRoute returnRoute);

    Task<ReturnFile> GetFECSVFile(string[] selectedPupils, string[] selectedDownloadOptions, bool confirmationGiven, AzureFunctionHeaderDetails azureFunctionHeaderDetails, ReturnRoute returnRoute);

    Task<ReturnFile> GetTABFile(string[] selectedPupils, string[] sortOrder, string[] selectedDownloadOptions, bool confirmationGiven, AzureFunctionHeaderDetails azureFunctionHeaderDetails, ReturnRoute returnRoute);

    Task<IEnumerable<CheckDownloadDataType>> CheckForNoDataAvailable(string[] selectedPupils, string[] sortOrder, string[] selectedDownloadOptions, AzureFunctionHeaderDetails azureFunctionHeaderDetails);

    Task<IEnumerable<DownloadUlnDataType>> CheckForFENoDataAvailable(string[] selectedPupils, string[] selectedDownloadOptions, AzureFunctionHeaderDetails azureFunctionHeaderDetails);

    Task<ReturnFile> GetPupilPremiumCSVFile(string[] selectedPupils, string[] sortOrder, bool confirmationGiven, AzureFunctionHeaderDetails azureFunctionHeaderDetails, ReturnRoute returnRoute, UserOrganisation userOrganisation = null);

    Task<IEnumerable<MetaDataDownload>> GetGlossaryMetaDataDownloadList();

    Task GetGlossaryMetaDataDownFileAsync(string fileName, Stream stream, AzureFunctionHeaderDetails azureFunctionHeaderDetails);
}
