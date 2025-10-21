namespace DfE.GIAP.Service.DsiApiClient;

public interface IDsiHttpClientProvider
{
    HttpClient CreateHttpClient();
}
