using System.Text;
using DfE.GIAP.Web.Features.Auth.Infrastructure.Config;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DfE.GIAP.Web.Features.Auth.Infrastructure;

public class SymmetricSigningCredentialsProvider : ISigningCredentialsProvider
{
    private readonly DsiOptions _dsiOptions;

    public SymmetricSigningCredentialsProvider(IOptions<DsiOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options.Value);
        _dsiOptions = options.Value;
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
