using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Service.DsiApiClient;
using DfE.GIAP.Service.Tests.DsiApiClient.TestDoubles;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using Xunit;

namespace DfE.GIAP.Service.Tests.DsiApiClient
{
    public class DsiHttpClientProviderTests
    {
        [Fact]
        public void DsiHttpClientProvider_throw_exception_null_Config()
        {
            // arrange
            ISecurityKeyProvider securityKeyProviderMock =
                SecurityKeyProviderTestDoubles.MockFor(
                    securityAlgorithm: SecurityKeyProviderTestDoubles.SymmetricSecurityAlgorithmFake,
                    securityKeyInstance: SecurityKeyProviderTestDoubles.SymmetricSecurityKeyInstanceFake);

            HttpClient httpClientMock =
                DsiHttpClientProviderTestDouble.HttpClientMock().Object;

            // Act
            Assert.Throws<ArgumentNullException>(() =>
                new DsiHttpClientProvider(httpClientMock, null, securityKeyProviderMock));
        }

        [Fact]
        public void DsiHttpClientProvider_throw_exception_null_securitytokendescriptior()
        {
            // arrange
            IOptions<AzureAppSettings> configurationMock = ConfigurationTestDoubles.MockForOptions(new AzureAppSettings()
            {
                DsiAuthorisationUrl = "https://www.example.org/"
            });

            HttpClient httpClientMock =
                DsiHttpClientProviderTestDouble.HttpClientMock().Object;

            // Act
            Assert.Throws<ArgumentNullException>(() =>
                new DsiHttpClientProvider(httpClientMock, configurationMock, null));
        }

        [Fact]
        public void DsiHttpClientProvider_throw_exception_null_httpClient()
        {
            // arrange
            IOptions<AzureAppSettings> configurationMock = ConfigurationTestDoubles.MockForOptions(new AzureAppSettings()
            {
                DsiAuthorisationUrl = "https://www.example.org/"
            });

            ISecurityKeyProvider securityKeyProviderMock =
                SecurityKeyProviderTestDoubles.MockFor(
                    securityAlgorithm: SecurityKeyProviderTestDoubles.SymmetricSecurityAlgorithmFake,
                    securityKeyInstance: SecurityKeyProviderTestDoubles.SymmetricSecurityKeyInstanceFake);

            // Act
            Assert.Throws<ArgumentNullException>(() =>
                new DsiHttpClientProvider(null, configurationMock, securityKeyProviderMock));
        }

        [Fact]
        public void CreateHttpClient_with_symmetric_signed_access_token_returns_correctly_configured_jwt()
        {
            // Arrange
            ISecurityKeyProvider securityKeyProviderMock =
                SecurityKeyProviderTestDoubles.MockFor(
                    securityAlgorithm: SecurityKeyProviderTestDoubles.SymmetricSecurityAlgorithmFake,
                    securityKeyInstance: SecurityKeyProviderTestDoubles.SymmetricSecurityKeyInstanceFake);

            IOptions<AzureAppSettings> configurationMock = ConfigurationTestDoubles.MockForOptions(new AzureAppSettings()
            {
                DsiAuthorisationUrl = "https://www.example.org/"
            });

            var dsiService = new DsiHttpClientProvider(
                    new HttpClient(), configurationMock, securityKeyProviderMock);

            // Act
            var httpClient = dsiService.CreateHttpClient();

            // Assert
            Assert.NotNull(httpClient);
            Assert.IsType<HttpClient>(httpClient);
            Assert.Equal("https://www.example.org/", httpClient.BaseAddress.ToString());
            Assert.Equal("HS256", JwtSecurityTokenHelper.ExtractJwtSecurityTokenFromClientHeader(httpClient).SignatureAlgorithm);
        }

        [Fact]
        public void CreateHttpClient_with_asymmetric_signed_access_token_returns_correctly_configured_jwt()
        {
            // Arrange
            ISecurityKeyProvider securityKeyProviderMock =
                SecurityKeyProviderTestDoubles.MockFor(
                    securityAlgorithm: SecurityKeyProviderTestDoubles.AsymmetricSecurityAlgorithmFake,
                    securityKeyInstance: SecurityKeyProviderTestDoubles.AsymmetricSecurityKeyInstanceFake);

            IOptions<AzureAppSettings> configurationMock = ConfigurationTestDoubles.MockForOptions(new AzureAppSettings()
            {
                DsiAuthorisationUrl = "https://www.example.org/"
            });

            var dsiService = new DsiHttpClientProvider(
                    new HttpClient(), configurationMock, securityKeyProviderMock);

            // Act
            var httpClient = dsiService.CreateHttpClient();

            // Assert
            Assert.NotNull(httpClient);
            Assert.IsType<HttpClient>(httpClient);
            Assert.Equal("https://www.example.org/", httpClient.BaseAddress.ToString());
            Assert.Equal("RS256", JwtSecurityTokenHelper.ExtractJwtSecurityTokenFromClientHeader(httpClient).SignatureAlgorithm);
        }

    }
}
