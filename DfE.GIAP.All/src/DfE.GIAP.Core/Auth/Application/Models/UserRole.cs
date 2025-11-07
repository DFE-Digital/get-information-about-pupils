namespace DfE.GIAP.Core.Auth.Application.Models;

/// <summary>
/// Represents a role assigned to a user.
/// </summary>
public class UserRole
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
