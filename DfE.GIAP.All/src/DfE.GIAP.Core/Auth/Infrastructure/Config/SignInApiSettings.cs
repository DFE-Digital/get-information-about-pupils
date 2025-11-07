namespace DfE.GIAP.Core.Auth.Infrastructure.Config;

public class SignInApiSettings
{
    public string ServiceId { get; set; } = string.Empty;
    public string AuthorisationUrl { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ApiClientSecret { get; set; } = string.Empty;

    public string GetUserProfileUrl { get; set; } = string.Empty;
    public string GetUserOrganisationUrl { get; set; } = string.Empty;
    public string GetUserOrganisationsUrl { get; set; } = string.Empty;
}
