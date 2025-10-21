using System.Security.Claims;
using DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils;
using DfE.GIAP.Web.Extensions;

namespace DfE.GIAP.Web.Helpers;

public sealed class HttpClaimsAuthorisationContext : IAuthorisationContext
{
    private readonly ClaimsPrincipal _user;

    public HttpClaimsAuthorisationContext(ClaimsPrincipal user)
    {
        _user = user;
    }

    public string Role => _user.GetUserRole();
    public bool IsDfeUser => _user.IsDfeUser();
    public int StatutoryAgeLow => _user.GetOrganisationLowAge();
    public int StatutoryAgeHigh => _user.GetOrganisationHighAge();
    public IReadOnlyCollection<string> Claims => _user.Claims.Select(c => c.Type).Distinct().ToList();

}
