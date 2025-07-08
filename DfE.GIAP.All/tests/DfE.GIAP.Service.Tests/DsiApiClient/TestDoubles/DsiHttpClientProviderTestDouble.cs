using DfE.GIAP.Service.DsiApiClient;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DfE.GIAP.Service.Tests.DsiApiClient.TestDoubles
{
    public static class DsiHttpClientProviderTestDouble
    {
        public static Mock<IDsiHttpClientProvider> DsiHttpClientProviderMock() => new();
        public static Mock<HttpClient> HttpClientMock() => new();

        public static IDsiHttpClientProvider MockFor(HttpClient httpClient)
        {
            var dsiHttpClientProviderMock = DsiHttpClientProviderMock();
            var dsiHttpClient = httpClient;
            dsiHttpClientProviderMock
                .Setup(dsiHttpClientProvider =>
                    dsiHttpClientProvider.CreateHttpClient())
                        .Returns(dsiHttpClient);

            return dsiHttpClientProviderMock.Object;
        }

        public static HttpClient MockDsiHttpClient(string responseObject, HttpStatusCode httpStatusCode)
        {
            const string BaseUrl = "https://pp-api.signin.education.gov.uk/";
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage{
                StatusCode = httpStatusCode,
                Content = new StringContent(responseObject),
            };

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(response);

            return new HttpClient(handlerMock.Object){
                BaseAddress = new System.Uri(BaseUrl)
            };
        }
    }
}
