using System.Text;
using DfE.GIAP.Core.Auth.Infrastructure.Config;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DfE.GIAP.Core.Auth.Infrastructure;

public class SymmetricSigningCredentialsProvider : ISigningCredentialsProvider
{
    private readonly DsiOptions _dsiOptions;

    public SymmetricSigningCredentialsProvider(IOptions<DsiOptions> options)
    {
        _dsiOptions = options.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public SigningCredentials GetSigningCredentials()
    {
        if (string.IsNullOrWhiteSpace(_dsiOptions.ApiClientSecret))
        {
            throw new SecurityTokenSignatureKeyNotFoundException("Missing DSI API client secret.");
        }

        byte[] keyBytes = Encoding.ASCII.GetBytes(_dsiOptions.ApiClientSecret);
        SymmetricSecurityKey key = new(keyBytes);
        return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }
}
