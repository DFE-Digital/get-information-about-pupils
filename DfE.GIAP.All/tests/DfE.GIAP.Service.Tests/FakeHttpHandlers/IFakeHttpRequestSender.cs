using System.Net.Http;

namespace DfE.GIAP.Service.Tests.FakeHttpHandlers
{
    public interface IFakeHttpRequestSender
    {
        HttpResponseMessage Send(HttpRequestMessage request);
    }
}
