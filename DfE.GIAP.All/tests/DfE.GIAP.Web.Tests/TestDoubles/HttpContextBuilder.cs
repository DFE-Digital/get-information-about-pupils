using System.Security.Claims;
using DfE.GIAP.Domain.Models.User;
using DfE.GIAP.Web.Constants;
using Microsoft.AspNetCore.Http;

namespace DfE.GIAP.Web.Tests.TestDoubles;

public sealed class HttpContextBuilder
{
    private readonly List<Claim> _claims = [];
    private bool _isAdmin = false;

    private HttpContextBuilder() { }
    public HttpContextBuilder WithUserId(string userId)
    {
        _claims.Add(new Claim(CustomClaimTypes.UserId, userId));
        return this;
    }

    public HttpContextBuilder WithOrganisationAgeRange(int lowAge, int highAge)
    {
        _claims.Add(
            new Claim(
                CustomClaimTypes.OrganisationLowAge,
                lowAge.ToString()));
        _claims.Add(
            new Claim(
                CustomClaimTypes.OrganisationHighAge,
                highAge.ToString()));
        return this;
    }

    public HttpContextBuilder AsAdmin()
    {
        _isAdmin = true;
        return this;
    }

    public DefaultHttpContext Build()
    {
        ClaimsIdentity identity = new(_claims, "TestAuth");

        if (_isAdmin)
        {
            identity.AddClaim(
                new Claim(
                    ClaimTypes.Role,
                    Roles.Admin));
        }

        ClaimsPrincipal principal = new(identity);

        return new DefaultHttpContext
        {
            User = principal
        };
    }

    internal static HttpContextBuilder Create() => new();
}
