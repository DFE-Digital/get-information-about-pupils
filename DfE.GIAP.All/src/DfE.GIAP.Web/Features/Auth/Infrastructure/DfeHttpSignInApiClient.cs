using DfE.GIAP.Web.Features.Auth.Application;
using DfE.GIAP.Web.Features.Auth.Application.Models;

namespace DfE.GIAP.Web.Features.Auth.Infrastructure;

public class DfeHttpSignInApiClient : IDfeSignInApiClient
{
    private readonly HttpClient _httpClient;

    public DfeHttpSignInApiClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
    }

    public async Task<UserAccess?> GetUserInfo(string serviceId, string organisationId, string userId)
    {
        string url = $"/services/{serviceId}/organisations/{organisationId}/users/{userId}";
        HttpResponseMessage response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserAccess>();
    }

    public async Task<Organisation?> GetUserOrganisation(string userId, string organisationId)
    {
        // Fetch the list of organisations
        List<Organisation> organisations = await GetUserOrganisations(userId);
        return organisations?.Find(o => o.Id == organisationId);
    }

    public async Task<List<Organisation>> GetUserOrganisations(string userId)
    {
        string url = $"/users/{userId}/organisations";

        HttpResponseMessage response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<Organisation>>() ?? new List<Organisation>();
    }
}
