namespace DfE.GIAP.Web.Features.Auth.Application.Models;
#nullable enable
/// <summary>
/// Represents the access rights a user has to a service and organisation.
/// </summary>
public class UserAccess
{
    public Guid UserId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid OrganisationId { get; set; }
    public IEnumerable<UserRole>? Roles { get; set; }
    public IEnumerable<KeyValuePair>? Identifiers { get; set; }
}

/// <summary>
/// Simple key/value pair used for identifiers.
/// </summary>
public class KeyValuePair
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
