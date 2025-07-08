using DfE.GIAP.Common.AppSettings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace DfE.GIAP.Service.DsiApiClient
{
    public class SymmetricSecurityKeyProvider : ISecurityKeyProvider
    {
        private readonly AzureAppSettings _appSettings;

        public SymmetricSecurityKeyProvider(IOptions<AzureAppSettings> appSettings)
        {
            _appSettings = appSettings?.Value ??
                throw new ArgumentNullException(nameof(appSettings));
        }

        public SecurityKey SecurityKeyInstance => new SymmetricSecurityKey(GetEncodedDsiSecret);
        public string SecurityAlgorithm => SecurityAlgorithms.HmacSha256Signature;

        private byte[] GetEncodedDsiSecret =>
            string.IsNullOrWhiteSpace(_appSettings.DsiApiClientSecret) ?
            throw new SecurityTokenSignatureKeyNotFoundException("Unable to locate required signing key.") :
            Encoding.ASCII.GetBytes(_appSettings.DsiApiClientSecret);
    }
}
