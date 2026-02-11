using System.Security.Claims;
using DfE.GIAP.Core.Downloads.Application.Availability.Access.Policies;
using DfE.GIAP.Web.Extensions;

namespace DfE.GIAP.Web.Helpers;

public sealed class HttpClaimsAuthorisationContext : IAuthorisationContext
{
    private readonly ClaimsPrincipal _user;

    public HttpClaimsAuthorisationContext(ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user);
        _user = user;
    }

    public bool IsAdminUser => _user.IsAdmin();
    public bool IsDfeUser => _user.IsDfeUser();
    public bool IsEstablishment => _user.IsOrganisationEstablishment();
    public bool IsLAUser => _user.IsOrganisationLocalAuthority();
    public bool IsMatUser => _user.IsOrganisationMultiAcademyTrust();
    public bool IsSatUser => _user.IsOrganisationSingleAcademyTrust();
    public bool AnyAgeUser => _user.IsOrganisationAllAges();

    public int StatutoryAgeLow => _user.GetOrganisationLowAge();
    public int StatutoryAgeHigh => _user.GetOrganisationHighAge();
    public IReadOnlyCollection<string> Claims => _user.Claims.Select(c => c.Type).Distinct().ToList();

}
