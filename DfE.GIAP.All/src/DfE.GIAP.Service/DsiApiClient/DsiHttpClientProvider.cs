using DfE.GIAP.Common.AppSettings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;

namespace DfE.GIAP.Service.DsiApiClient
{
    public class DsiHttpClientProvider : IDsiHttpClientProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ISecurityKeyProvider _securityKeyProvider;
        private readonly AzureAppSettings _appSettings;

        public DsiHttpClientProvider(HttpClient httpClient, IOptions<AzureAppSettings> appSettings, ISecurityKeyProvider securityKeyProvider)
        {
            _httpClient = httpClient ??
                throw new ArgumentNullException(nameof(httpClient));
            _appSettings = appSettings?.Value ??
                throw new ArgumentNullException(nameof(appSettings));
            _securityKeyProvider = securityKeyProvider ??
                throw new ArgumentNullException(nameof(securityKeyProvider));
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
}
