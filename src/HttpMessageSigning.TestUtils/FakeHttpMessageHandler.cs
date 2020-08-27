using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.TestUtils {
    public class FakeHttpMessageHandler : HttpMessageHandler {
        public FakeHttpMessageHandler(HttpResponseMessage responseToReturn) {
            ResponseToReturn = responseToReturn ?? throw new ArgumentNullException(nameof(responseToReturn));
        }

        public HttpResponseMessage ResponseToReturn { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            return Task.FromResult(ResponseToReturn);
        }
    }
}