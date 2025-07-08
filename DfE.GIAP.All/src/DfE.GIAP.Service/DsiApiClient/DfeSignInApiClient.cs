using DfE.GIAP.Domain.Models.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DfE.GIAP.Service.DsiApiClient
{
    public class DfeSignInApiClient: IDfeSignInApiClient
    {
        private readonly HttpClient _dsiHttpClient;

        public DfeSignInApiClient(IDsiHttpClientProvider dsiHttpClientProvider)
        {
            if (dsiHttpClientProvider == null)
                throw new ArgumentNullException(nameof(dsiHttpClientProvider));

            _dsiHttpClient = dsiHttpClientProvider.CreateHttpClient();
        }

        public Task<UserAccess> GetUserInfo(
            string serviceId, string organisationId, string userId) =>
                GetDsiModel<UserAccess>(
                    $"/services/{serviceId}/organisations/{organisationId}/users/{userId}");

        public Task<Organisation> GetUserOrganisation(
            string userId, string organisationId) =>
                GetUserOrganisations(userId).ContinueWith(organisations =>
                    organisations.Result?.Find(organisation => organisation.Id == organisationId));

        public Task<List<Organisation>> GetUserOrganisations(
            string userId) =>
                GetDsiModel<List<Organisation>>(
                    $"/users/{userId}/organisations");

        private async Task<TModel> GetDsiModel<TModel>(string requestUri) where TModel : class, new()
        {
            TModel dsiModel = default;
            var response = await _dsiHttpClient.GetAsync(requestUri);

            if (response.IsSuccessStatusCode){
                string responseContent = await response.Content.ReadAsStringAsync();
                dsiModel = JsonConvert.DeserializeObject<TModel>(responseContent);
            }
            return dsiModel;
        }
    }
}
