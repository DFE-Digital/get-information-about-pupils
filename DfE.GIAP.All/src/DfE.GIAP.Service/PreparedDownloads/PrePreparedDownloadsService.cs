using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Models.PrePreparedDownloads;
using DfE.GIAP.Service.ApiProcessor;
using DfE.GIAP.Service.BlobStorage;
using DfE.GIAP.Service.Helpers;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace DfE.GIAP.Service.PreparedDownloads
{
    public class PrePreparedDownloadsService : IPrePreparedDownloadsService 
    {  
        private readonly IApiService _apiProcessorService;
        private AzureAppSettings _azureAppSettings;
        private readonly IBlobStorageService _prePreparedDownloadsBlobStorageService;

        public PrePreparedDownloadsService(IOptions<AzureAppSettings> azureFunctionUrls,
                                           IApiService apiProcessorService,
                                           IBlobStorageService prePreparedDownloadsBlobStorageService)
        {
            _apiProcessorService = apiProcessorService;
            _azureAppSettings = azureFunctionUrls.Value;
            _prePreparedDownloadsBlobStorageService = prePreparedDownloadsBlobStorageService;
        }

        public async Task<IList<PrePreparedDownloads>> GetPrePreparedDownloadsList(AzureFunctionHeaderDetails azureFunctionHeaderDetails,
                                                                                  bool isOrganisationLocalAuthority = false, 
                                                                                  bool isOrganisationMultiAcademyTrust = false,
                                                                                  bool isOrganisationEstablishment = false, 
                                                                                  bool isOrganisationSingleAcademyTrust = false, 
                                                                                  string getEstablishmentNumber = "", 
                                                                                  string uniqueIdentifier = "", 
                                                                                  string getLocalAuthorityNumber="", 
                                                                                  string uniqueReferenceNumber = "")
        {
            
            var downloadPrepreparedFilesUrl = _azureAppSettings.DownloadPrepreparedFilesUrl;

            var requestBody = new PrePreparedDownloadsRequestBody { IsOrganisationLocalAuthority = isOrganisationLocalAuthority.ToString().ToLower(),
                                                                   IsOrganisationMultiAcademyTrust = isOrganisationMultiAcademyTrust.ToString().ToLower(),
                                                                   IsOrganisationEstablishment = isOrganisationEstablishment.ToString().ToLower(),
                                                                   IsOrganisationSingleAcademyTrust = isOrganisationSingleAcademyTrust.ToString().ToLower(),
                                                                   EstablishmentNumber = getEstablishmentNumber,
                                                                   UniqueIdentifier = uniqueIdentifier,
                                                                   LocalAuthorityNumber = getLocalAuthorityNumber,
                                                                   UniqueReferenceNumber = uniqueReferenceNumber};
            var response = await _apiProcessorService.PostAsync<PrePreparedDownloadsRequestBody, IList<PrePreparedDownloads>>(downloadPrepreparedFilesUrl.ConvertToUri(), requestBody, azureFunctionHeaderDetails).ConfigureAwait(false);

            return response;
           
        }

        public Task PrePreparedDownloadsFileAsync(string fileName, Stream stream, AzureFunctionHeaderDetails azureFunctionHeaderDetails, bool IsOrganisationLocalAuthority = false, 
                                                  bool IsOrganisationMultiAcademyTrust = false, bool IsOrganisationEstablishment = false, 
                                                  bool IsOrganisationSingleAcademyTrust = false, string EstablishmentNumber = "", 
                                                  string UniqueIdentifier = "", string LocalAuthorityNumber = "", string UniqueReferenceNumber = "")
        {    
            var directory =  BuildFolderPath(IsOrganisationLocalAuthority, IsOrganisationMultiAcademyTrust,  IsOrganisationEstablishment, IsOrganisationSingleAcademyTrust, EstablishmentNumber, UniqueIdentifier, LocalAuthorityNumber, UniqueReferenceNumber);
            var file = _prePreparedDownloadsBlobStorageService.DownloadFileAsync(directory.Result, fileName, stream, azureFunctionHeaderDetails);
            return file;           
        }

        public async Task<string> BuildFolderPath(bool IsOrganisationLocalAuthority = false, bool IsOrganisationMultiAcademyTrust = false, bool IsOrganisationEstablishment = false, bool IsOrganisationSingleAcademyTrust = false, string EstablishmentNumber = "", string UniqueIdentifier = "", string LocalAuthorityNumber = "", string UniqueReferenceNumber = "")
        {
            var directoryPath = "";


            if (IsOrganisationLocalAuthority)
            {
                string folderName = "LA";
                directoryPath = $"{folderName}/{LocalAuthorityNumber}/";
            }
            else if (IsOrganisationMultiAcademyTrust)
            {
                string folderName = "MAT";
                directoryPath = $"{folderName}/{UniqueIdentifier}/";
            }
            else if (IsOrganisationSingleAcademyTrust)
            {
                string folderName = "SAT";
                directoryPath = $"{folderName}/{UniqueIdentifier}/";

            }
            else if (IsOrganisationEstablishment)
            {
                string folderName = "School";
                directoryPath = $"{folderName}/{UniqueReferenceNumber}/";

            }
            return directoryPath;
        }
    }
}
