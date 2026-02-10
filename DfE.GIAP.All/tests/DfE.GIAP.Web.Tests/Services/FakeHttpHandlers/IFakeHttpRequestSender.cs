using System.Net.Http;

namespace DfE.GIAP.Web.Tests.Services.FakeHttpHandlers;

public interface IFakeHttpRequestSender
{
    HttpResponseMessage Send(HttpRequestMessage request);
}
