using DfE.GIAP.Common.AppSettings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;

namespace DfE.GIAP.Service.DsiApiClient;

public class DsiHttpClientProvider : IDsiHttpClientProvider
{
    private readonly HttpClient _httpClient;
    private readonly ISecurityKeyProvider _securityKeyProvider;
    private readonly AzureAppSettings _appSettings;

    public DsiHttpClientProvider(
        HttpClient httpClient,
        IOptions<AzureAppSettings> appSettings,
        ISecurityKeyProvider securityKeyProvider)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(appSettings);
        ArgumentNullException.ThrowIfNull(securityKeyProvider);
        _httpClient = httpClient;
        _appSettings = appSettings?.Value;
        _securityKeyProvider = securityKeyProvider;
    }

    public HttpClient CreateHttpClient()
    {
        const string TokenMediaType = "application/json";
        const string TokenScheme = "Bearer";

        string encodedDsiAccessToken = CreateEncodedDsiAccessToken();
        string dsiAuthorisationUrl = _appSettings.DsiAuthorisationUrl.TrimEnd('/');

        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(TokenMediaType));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TokenScheme, encodedDsiAccessToken);
        _httpClient.BaseAddress = new Uri(dsiAuthorisationUrl);

        return _httpClient;
    }

    private string CreateEncodedDsiAccessToken() =>
        new JwtSecurityTokenHandler()
            .CreateEncodedJwt(
                new SecurityTokenDescriptor
                {
                    Issuer = _appSettings.DsiClientId,
                    Audience = _appSettings.DsiAudience,
                    SigningCredentials =
                        new SigningCredentials(
                            _securityKeyProvider.SecurityKeyInstance,
                            _securityKeyProvider.SecurityAlgorithm)
                });
}
