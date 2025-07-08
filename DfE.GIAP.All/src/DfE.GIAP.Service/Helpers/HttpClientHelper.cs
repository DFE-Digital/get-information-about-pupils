using DfE.GIAP.Common.Constants.AzureFunction;
using DfE.GIAP.Domain.Models.Common;
using System.Net.Http;

namespace DfE.GIAP.Service.Helpers
{
    public static class HttpClientHelper
    {

        public static void ConfigureHeaders(this HttpClient httpClient,
                                            AzureFunctionHeaderDetails azureFunctionHeaderDetails)
        {
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add(HeaderDetails.ClientId, azureFunctionHeaderDetails.ClientId);
            httpClient.DefaultRequestHeaders.Add(HeaderDetails.SessionId, azureFunctionHeaderDetails.SessionId);
        }
    }
}
