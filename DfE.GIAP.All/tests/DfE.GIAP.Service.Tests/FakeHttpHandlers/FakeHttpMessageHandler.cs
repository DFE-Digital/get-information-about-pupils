using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DfE.GIAP.Service.Tests.FakeHttpHandlers
{
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly IFakeHttpRequestSender _fakeHttpRequestSender;

        public FakeHttpMessageHandler(IFakeHttpRequestSender fakeHttpRequestSender) {
            _fakeHttpRequestSender = fakeHttpRequestSender;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_fakeHttpRequestSender.Send(request));        
        }
    }
}
