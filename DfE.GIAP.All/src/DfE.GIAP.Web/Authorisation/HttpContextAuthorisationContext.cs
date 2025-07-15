using DfE.GIAP.Core.MyPupils.Application.UseCase.GetMyPupils;
using DfE.GIAP.Domain.Models.User;
using DfE.GIAP.Web.Constants;

namespace DfE.GIAP.Web.Authorisation;

public sealed class HttpContextAuthorisationContext : IAuthorisationContext
{
    private readonly HttpContext _context;

    public HttpContextAuthorisationContext(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;

        // Existing helper logic
        // Capture immediatly async work HttpContext ConfigureAwait issues with SycnhronisationContext. Note write tests for this.
        UserId =
            _context.User.Claims.FirstOrDefault(
                (c) => c.Type == CustomClaimTypes.UserId)?.Value ?? string.Empty;

        LowAge =
            int.TryParse(
                _context.User.Claims.FirstOrDefault(
                    (c) => c.Type == CustomClaimTypes.OrganisationLowAge)?.Value, out int lowAge) ? lowAge : 0; // Default to 0 if claim parse fails

        HighAge =
            int.TryParse(
                _context.User.Claims.FirstOrDefault(
                    (c) => c.Type == CustomClaimTypes.OrganisationHighAge)?.Value, out int highAge) ? highAge : 0; // Default to 0 if claim parse fails

        IsAdministrator = _context.User.IsInRole(Roles.Admin);
    }

    public string UserId { get; init; }

    public int LowAge { get; init; }

    public int HighAge { get; init; }

    public bool IsAdministrator { get; init; }
}

