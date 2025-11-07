using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DfE.GIAP.Core.Auth.Application;
using DfE.GIAP.Core.Auth.Application.Models;
using DfE.GIAP.Core.Auth.Infrastructure.Config;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DfE.GIAP.Core.Auth.Infrastructure;

public class DfeSignInApiClient : IDfeSignInApiClient
{
    private readonly HttpClient _httpClient;
    private readonly SignInApiSettings _settings;
    private readonly ISigningCredentialsProvider _credentialsProvider;

    public DfeSignInApiClient(HttpClient httpClient, IOptions<SignInApiSettings> options, ISigningCredentialsProvider credentialsProvider)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _credentialsProvider = credentialsProvider;

        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        string token = new JwtSecurityTokenHandler().CreateEncodedJwt(
            new SecurityTokenDescriptor
            {
                Issuer = _settings.ClientId,
                Audience = _settings.Audience,
                SigningCredentials = _credentialsProvider.GetSigningCredentials()
            });

        _httpClient.BaseAddress = new Uri(_settings.AuthorisationUrl.TrimEnd('/'));
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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
