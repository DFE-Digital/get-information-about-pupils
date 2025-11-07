namespace DfE.GIAP.Core.Auth.Infrastructure.Config;

/// <summary>
/// Settings required for OIDC authentication.
/// </summary>
public class DsiOptions
{
    public const string SectionName = "DsiOptions";

    public string ServiceId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string ApiClientSecret { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string MetadataAddress { get; set; } = string.Empty;
    public string AuthorisationUrl { get; set; } = string.Empty;
    public string CallbackPath { get; set; } = string.Empty;
    public string SignedOutCallbackPath { get; set; } = string.Empty;
    public int SessionTimeoutMinutes { get; set; }

}
