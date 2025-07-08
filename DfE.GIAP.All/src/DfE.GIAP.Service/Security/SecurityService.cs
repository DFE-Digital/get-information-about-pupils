using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Domain.Models.SecurityReports;
using DfE.GIAP.Service.ApiProcessor;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DfE.GIAP.Service.Security
{
    public class SecurityService : ISecurityService
    {
        private readonly IApiService _apiProcessorService;
        private AzureAppSettings _azureAppSettings;

        public SecurityService(IApiService apiProcessorService, IOptions<AzureAppSettings> azureAppSettings)
        {
            _apiProcessorService = apiProcessorService;
            _azureAppSettings = azureAppSettings.Value;
        }

        public async Task<IList<LocalAuthority>> GetAllLocalAuthorities()
        {
            var url = _azureAppSettings.QueryLAGetAllUrl;
            var response = await _apiProcessorService.GetToListAsync<LocalAuthority>(url.ConvertToUri()).ConfigureAwait(false);
            return response;
        }

        public async Task<IList<AcademyTrust>> GetAcademyTrusts(List<string> docTypes, string id = null)
        {
            string url = _azureAppSettings.GetAcademiesURL;
            AcademyRequest requestBody = new()
            {
                DocTypes = docTypes,
                Id = id,
                IncludeEstablishments = false
            };

            List<AcademyTrust> response = await _apiProcessorService.PostAsync<AcademyRequest, List<AcademyTrust>>(url.ConvertToUri(), requestBody, null).ConfigureAwait(false);
            return response;
        }

        public async Task<IList<LocalAuthority>> GetAllFurtherEducationOrganisations()
        {
            var url = _azureAppSettings.GetAllFurtherEducationURL;
            var response = await _apiProcessorService.GetToListAsync<LocalAuthority>(url.ConvertToUri()).ConfigureAwait(false);
            return response;
        }

        public async Task<IList<Establishment>> GetEstablishmentsByAcademyTrustCode(List<string> docTypes, string academyTrustCode)
        {
            var url = _azureAppSettings.GetAcademiesURL;
            var requestBody = new AcademyRequest { DocTypes = docTypes, Id = academyTrustCode, IncludeEstablishments = true };

            var response = await _apiProcessorService.PostAsync<AcademyRequest, List<AcademyTrust>>(url.ConvertToUri(), requestBody, null).ConfigureAwait(false);
            return response?[0].Establishments.ToList();
        }

        public async Task<IList<Establishment>> GetEstablishmentsByOrganisationCode(string docType, string organisationCode)
        {
            var url = docType.Equals("FE") ? _azureAppSettings.GetFurtherEducationByCodeURL : _azureAppSettings.QueryLAByCodeUrl;
            var requestBody = new EstablishmentRequest { Type = docType, Code = organisationCode };

            var response = await _apiProcessorService.PostAsync<EstablishmentRequest, EstablishmentResponse>(url.ConvertToUri(), requestBody, null).ConfigureAwait(false);
            return response.Establishments.ToList();
        }
    }
}
