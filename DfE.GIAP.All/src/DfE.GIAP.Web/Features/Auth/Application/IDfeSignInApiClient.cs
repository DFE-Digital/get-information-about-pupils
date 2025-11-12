using DfE.GIAP.Web.Features.Auth.Application.Models;

namespace DfE.GIAP.Web.Features.Auth.Application;

public interface IDfeSignInApiClient
{
    Task<UserAccess?> GetUserInfo(string serviceId, string organisationId, string userId);
    Task<Organisation?> GetUserOrganisation(string userId, string organisationId);
    Task<List<Organisation>> GetUserOrganisations(string userId);
}
