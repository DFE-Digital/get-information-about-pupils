using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Models.Common
{
    [ExcludeFromCodeCoverage]
    public class AzureFunctionHeaderDetails
    {
        public string ClientId { get; set; }

        public string SessionId { get; set; }

        public static AzureFunctionHeaderDetails Create(string clientId, string sessionId)
        {
            return new AzureFunctionHeaderDetails
            {
                ClientId = clientId,
                SessionId = sessionId
            };
        }
    }

}
