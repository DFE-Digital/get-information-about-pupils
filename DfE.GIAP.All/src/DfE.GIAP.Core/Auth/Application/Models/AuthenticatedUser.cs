namespace DfE.GIAP.Core.Auth.Application.Models;

/// <summary>
/// Represents the authenticated user within the Auth feature.
/// </summary>
public class AuthenticatedUser : BasicUser
{
    public bool IsAdmin { get; set; }
    public bool IsApprover { get; set; }
    public bool IsUser { get; set; }
}
