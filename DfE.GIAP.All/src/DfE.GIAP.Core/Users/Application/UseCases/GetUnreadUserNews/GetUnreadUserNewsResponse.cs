namespace DfE.GIAP.Core.Users.Application.UseCases.GetUnreadUserNews;

/// <summary>
/// Represents the response indicating whether there are unread user news updates.
/// </summary>
/// <param name="HasUpdates">A value indicating whether there are unread updates.
/// <see langword="true"/> if there are unread updates; otherwise,
/// <see langword="false"/>.</param>
public record GetUnreadUserNewsResponse(bool HasUpdates);

