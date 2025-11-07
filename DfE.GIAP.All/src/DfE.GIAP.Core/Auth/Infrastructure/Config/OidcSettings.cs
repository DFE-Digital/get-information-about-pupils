namespace DfE.GIAP.Core.Auth.Infrastructure.Config;

/// <summary>
/// Settings required for OIDC authentication.
/// </summary>
public class OidcSettings
{
    public string MetadataAddress { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public int SessionTimeoutMinutes { get; set; }
    public string CallbackPath { get; set; } = "/signin-oidc";
    public string SignedOutCallbackPath { get; set; } = "/signout-callback-oidc";
    public string ServiceId { get; set; } = string.Empty;
}
