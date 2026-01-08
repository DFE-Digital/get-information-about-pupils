using DfE.GIAP.Core.Common.Application;

namespace DfE.GIAP.Core.Users.Application.UseCases.UpdateLastLogin;

/// <summary>
/// Represents a request to update the last logged-in timestamp for a specific user.
/// </summary>
/// <remarks>This request is typically used to inform the system of the most recent login activity for a
/// user.</remarks>
/// <param name="UserId">The unique identifier of the user whose last logged-in timestamp is being updated. Cannot be null or empty.</param>
/// <param name="LastLoggedIn">The date and time of the user's last login, in UTC.</param>
public record UpdateLastLoggedInRequest(string UserId, DateTime LastLoggedIn) : IUseCaseRequest;
