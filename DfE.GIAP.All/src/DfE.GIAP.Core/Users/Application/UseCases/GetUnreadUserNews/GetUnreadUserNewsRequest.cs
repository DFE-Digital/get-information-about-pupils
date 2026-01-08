using DfE.GIAP.Core.Common.Application;

namespace DfE.GIAP.Core.Users.Application.UseCases.GetUnreadUserNews;

/// <summary>
/// Represents a request to retrieve unread news items for a specific user.
/// </summary>
/// <param name="UserId">The unique identifier of the user for whom unread news items are being requested.  This value cannot be null or
/// empty.</param>
public record GetUnreadUserNewsRequest(string UserId) : IUseCaseRequest<GetUnreadUserNewsResponse>;
