namespace DfE.GIAP.Core.Auth.Application.Models;

/// <summary>
/// Base user identity with only the unique identifier.
/// </summary>
public class BasicUser
{
    public string UserId { get; set; } = string.Empty;
}
