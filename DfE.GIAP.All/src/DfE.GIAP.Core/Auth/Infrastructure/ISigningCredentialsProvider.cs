using Microsoft.IdentityModel.Tokens;

namespace DfE.GIAP.Core.Auth.Infrastructure;

public interface ISigningCredentialsProvider
{
    SigningCredentials GetSigningCredentials();
}
