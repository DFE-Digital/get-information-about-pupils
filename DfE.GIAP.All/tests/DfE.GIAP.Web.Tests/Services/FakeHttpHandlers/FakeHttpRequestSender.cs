using System;
using System.Net.Http;

namespace DfE.GIAP.Service.Tests.FakeHttpHandlers
{
    public class FakeHttpRequestSender : IFakeHttpRequestSender
    {
        public HttpResponseMessage Send(HttpRequestMessage request) {
            throw new NotImplementedException("Set this up in the method with our mocking framework");
        }
    }
}
