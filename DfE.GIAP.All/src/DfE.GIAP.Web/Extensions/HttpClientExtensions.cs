using DfE.GIAP.Common.Constants.AzureFunction;
using DfE.GIAP.Domain.Models.Common;

namespace DfE.GIAP.Web.Extensions;

public static class HttpClientExtensions
{

    public static void ConfigureHeaders(this HttpClient httpClient,
                                        AzureFunctionHeaderDetails azureFunctionHeaderDetails)
    {
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add(HeaderDetails.ClientId, azureFunctionHeaderDetails.ClientId);
        httpClient.DefaultRequestHeaders.Add(HeaderDetails.SessionId, azureFunctionHeaderDetails.SessionId);
    }
}
