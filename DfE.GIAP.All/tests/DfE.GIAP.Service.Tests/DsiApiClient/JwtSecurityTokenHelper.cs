using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;

namespace DfE.GIAP.Service.Tests.DsiApiClient
{
    public static class JwtSecurityTokenHelper
    {
        public static JwtSecurityToken ExtractJwtSecurityTokenFromClientHeader(
            HttpClient httpClient) =>
                new JwtSecurityTokenHandler()
                    .ReadJwtToken(httpClient.DefaultRequestHeaders.Authorization.Parameter);
    }
}
