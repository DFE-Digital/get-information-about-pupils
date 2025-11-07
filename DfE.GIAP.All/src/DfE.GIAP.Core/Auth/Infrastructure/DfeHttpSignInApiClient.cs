using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DfE.GIAP.Core.Auth.Application;
using DfE.GIAP.Core.Auth.Application.Models;
using DfE.GIAP.Core.Auth.Infrastructure.Config;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DfE.GIAP.Core.Auth.Infrastructure;

public class DfeHttpSignInApiClient : IDfeSignInApiClient
{
    private readonly HttpClient _httpClient;
    private readonly DsiOptions __dsiOptions;
    private readonly ISigningCredentialsProvider _credentialsProvider;

    public DfeHttpSignInApiClient(
        HttpClient httpClient,
        IOptions<DsiOptions> options,
        ISigningCredentialsProvider credentialsProvider)
    {
        _httpClient = httpClient;
        __dsiOptions = options.Value;
        _credentialsProvider = credentialsProvider;

        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        string token = new JwtSecurityTokenHandler().CreateEncodedJwt(
            new SecurityTokenDescriptor
            {
                Issuer = __dsiOptions.ClientId,
                Audience = __dsiOptions.Audience,
                SigningCredentials = _credentialsProvider.GetSigningCredentials()
            });

        _httpClient.BaseAddress = new Uri(__dsiOptions.AuthorisationUrl.TrimEnd('/'));
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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
