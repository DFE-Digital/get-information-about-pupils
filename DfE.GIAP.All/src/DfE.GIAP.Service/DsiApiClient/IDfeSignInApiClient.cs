using DfE.GIAP.Domain.Models.User;

namespace DfE.GIAP.Service.DsiApiClient;

public interface IDfeSignInApiClient
{
    Task<UserAccess> GetUserInfo(string serviceId, string organisationId, string userId);
    Task<Organisation> GetUserOrganisation(string userId, string organisationId);
    Task<List<Organisation>> GetUserOrganisations(string userId);
}
