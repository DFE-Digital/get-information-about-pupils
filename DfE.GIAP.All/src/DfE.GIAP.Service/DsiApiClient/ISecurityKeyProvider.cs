using Microsoft.IdentityModel.Tokens;

namespace DfE.GIAP.Service.DsiApiClient
{
    public interface ISecurityKeyProvider
    {
        SecurityKey SecurityKeyInstance { get; }
        string SecurityAlgorithm { get; }
    }
}
