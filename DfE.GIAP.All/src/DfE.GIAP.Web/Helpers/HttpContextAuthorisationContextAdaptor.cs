using DfE.GIAP.Domain.Models.User;
using DfE.GIAP.Web.Constants;
using System.Security.Claims;

namespace DfE.GIAP.Web.Helpers;

public sealed class HttpContextAuthorisationContextAdaptor : IAuthorisationContext
{
    public HttpContextAuthorisationContextAdaptor(IHttpContextAccessor accessor)
    {
        ArgumentNullException.ThrowIfNull(accessor);
        ArgumentNullException.ThrowIfNull(accessor.HttpContext);

        ClaimsPrincipal user = accessor.HttpContext.User;

        UserId = user.Claims.FirstOrDefault(
            (c) => c.Type == CustomClaimTypes.UserId)?.Value ?? throw new ArgumentException("User id claim is empty");

        LowAge = int.TryParse(
            user.Claims.FirstOrDefault(
                    (c) => c.Type == CustomClaimTypes.OrganisationLowAge)?.Value, out int lowAge)
                ? lowAge : 0;

        HighAge = int.TryParse(
            user.Claims.FirstOrDefault(
                    (c) => c.Type == CustomClaimTypes.OrganisationHighAge)?.Value, out int highAge)
                ? highAge : 0;

        IsAdministrator = user.IsInRole(Roles.Admin);
    }

    public string UserId { get; init; }
    public int LowAge { get; init; }
    public int HighAge { get; init; }
    public bool IsAdministrator { get; init; }
}
