using System.Net.Http.Json;
using DfE.GIAP.Core.Auth.Application;
using DfE.GIAP.Core.Auth.Application.Models;
using DfE.GIAP.Core.Auth.Infrastructure.Config;

namespace DfE.GIAP.Core.Auth.Infrastructure;

public class DfeSignInApiClient : IDfeSignInApiClient
{
    private readonly HttpClient _httpClient;
    private readonly SignInApiSettings _settings;

    public DfeSignInApiClient(HttpClient httpClient, SignInApiSettings settings)
    {
        _httpClient = httpClient;
        _settings = settings;
    }

    public async Task<UserAccess?> GetUserInfo(string serviceId, string organisationId, string userId)
    {
        string url = $"{_settings.GetUserProfileUrl}?serviceId={serviceId}&organisationId={organisationId}&userId={userId}";
        HttpResponseMessage response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserAccess>();
    }

    public async Task<Organisation?> GetUserOrganisation(string userId, string organisationId)
    {
        string url = $"{_settings.GetUserOrganisationUrl}?userId={userId}&organisationId={organisationId}";
        HttpResponseMessage response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Organisation>();
    }

    public async Task<List<Organisation>> GetUserOrganisations(string userId)
    {
        string url = $"{_settings.GetUserOrganisationsUrl}?userId={userId}";
        HttpResponseMessage response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<Organisation>>() ?? new List<Organisation>();
    }
}
