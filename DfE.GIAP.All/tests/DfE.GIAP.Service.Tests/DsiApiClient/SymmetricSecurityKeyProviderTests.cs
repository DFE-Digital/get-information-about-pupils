using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Service.DsiApiClient;
using DfE.GIAP.Service.Tests.DsiApiClient.TestDoubles;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using Xunit;

namespace DfE.GIAP.Service.Tests.DsiApiClient
{
    public class SymmetricSecurityKeyProviderTests
    {
        [Fact]
        public void SymmetricSecurityTokenDescriptorProvider_throws_error_when_not_configured()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new SymmetricSecurityKeyProvider(appSettings: null));
        }

        [Fact]
        public void SecurityAlgorithm_returns_HmacSha256Signature_algorithm()
        {
            // arrange
            var appSettings = ConfigurationTestDoubles.OptionsMock<AzureAppSettings>();
            var symmetricSecurityKeyProvider = new SymmetricSecurityKeyProvider(appSettings: appSettings);

            // act
            string securityAlgorithm = symmetricSecurityKeyProvider.SecurityAlgorithm;

            // assert
            Assert.NotNull(securityAlgorithm);
            Assert.Equal("http://www.w3.org/2001/04/xmldsig-more#hmac-sha256", securityAlgorithm);
        }

        [Fact]
        public void SecurityKeyInstance_with_configured_DsiApiClientSecret_returns_symmetric_security_key()
        {
            // arrange
            IOptions<AzureAppSettings> configurationMock =
                ConfigurationTestDoubles.MockForOptions(new AzureAppSettings()
                {
                    DsiApiClientSecret = "test_secret_key"
                });

            var symmetricSecurityKeyProvider =
                new SymmetricSecurityKeyProvider(appSettings: configurationMock);

            // act
            SecurityKey securityKey = symmetricSecurityKeyProvider.SecurityKeyInstance;

            // assert
            Assert.NotNull(securityKey);
            Assert.IsType<SymmetricSecurityKey>(securityKey);
        }

        [Fact]
        public void SecurityKeyInstance_with_empty_configured_DsiApiClientSecret_throws_SecurityTokenSignatureKeyNotFoundException()
        {
            // arrange
            IOptions<AzureAppSettings> configurationMock =
               ConfigurationTestDoubles.MockForOptions(new AzureAppSettings()
               {
                   DsiApiClientSecret = string.Empty
               });

            var symmetricSecurityKeyProvider =
                new SymmetricSecurityKeyProvider(appSettings: configurationMock);

            // assert
            Assert.Throws<SecurityTokenSignatureKeyNotFoundException>(() =>
                symmetricSecurityKeyProvider.SecurityKeyInstance);
        }
    }
}
