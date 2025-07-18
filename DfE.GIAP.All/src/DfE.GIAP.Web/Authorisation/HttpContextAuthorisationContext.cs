using System.Security.Claims;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.AuthorisationContext;
using DfE.GIAP.Domain.Models.User;
using DfE.GIAP.Web.Constants;

namespace DfE.GIAP.Web.Authorisation;
public sealed class HttpContextAuthorisationContext : IAuthorisationContext
{
    public HttpContextAuthorisationContext(IHttpContextAccessor accessor)
    {
        ArgumentNullException.ThrowIfNull(accessor);
        ArgumentNullException.ThrowIfNull(accessor.HttpContext);

        ClaimsPrincipal user = accessor.HttpContext.User;

        UserId = user.Claims.FirstOrDefault(
            (c) => c.Type == CustomClaimTypes.UserId)?.Value; // TODO should we be validating that the User claim is not empty here, rather than sending it down as `null` which is what ClaimsPrincipalExtensions was doing previously
            // Can we judge how many UserProfiles signed in without an Id, e.g they were created through the Create path of Upsert / Get UserProfile. Use CreatedDate?

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
