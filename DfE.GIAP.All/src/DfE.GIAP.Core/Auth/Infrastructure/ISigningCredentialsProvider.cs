using Microsoft.IdentityModel.Tokens;

namespace DfE.GIAP.Core.Auth.Infrastructure;

/// <summary>
/// Defines a mechanism for retrieving signing credentials used to generate digital signatures for security tokens or
/// other cryptographic operations.
/// </summary>
public interface ISigningCredentialsProvider
{
    /// <summary>
    /// Retrieves the signing credentials used to generate digital signatures for security tokens.
    /// </summary>
    /// <returns>A <see cref="SigningCredentials"/> instance containing the cryptographic credentials for signing tokens.</returns>
    SigningCredentials GetSigningCredentials();
}
