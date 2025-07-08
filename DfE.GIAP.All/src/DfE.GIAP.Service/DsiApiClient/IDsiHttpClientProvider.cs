using System.Net.Http;

namespace DfE.GIAP.Service.DsiApiClient
{
    public interface IDsiHttpClientProvider
    {
        HttpClient CreateHttpClient();
    }
}
