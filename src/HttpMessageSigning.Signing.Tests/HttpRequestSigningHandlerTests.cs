using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.TestUtils;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Signing {
    public class HttpRequestSigningHandlerTests {
        private readonly IRequestSigner _requestSigner;
        private readonly HttpRequestSigningHandler _sut;
        private readonly HttpResponseMessage _responseFromInner;

        public HttpRequestSigningHandlerTests() {
            FakeFactory.Create(out _requestSigner);
            _responseFromInner = new HttpResponseMessage(HttpStatusCode.OK);
            
            _sut = new HttpRequestSigningHandler(_requestSigner) {
                InnerHandler = new FakeHttpMessageHandler(_responseFromInner)
            };
        }

        public class SendAsync : HttpRequestSigningHandlerTests, IDisposable {
            private readonly HttpClient _httpClient;
            private readonly HttpRequestMessage _request;

            public SendAsync() {
                _httpClient = new HttpClient(_sut);
                _request = new HttpRequestMessage(HttpMethod.Get, "https://www.dalion.eu");
            }

            [Fact]
            public async Task SignsRequestBeforeSending() {
                await _httpClient.SendAsync(_request);

                A.CallTo(() => _requestSigner.Sign(_request))
                    .MustHaveHappened();
            }

            [Fact]
            public async Task ReturnsResponseFromInnerHandler() {
                var actual = await _httpClient.SendAsync(_request);
                
                actual.Should().Be(_responseFromInner);
            }

            public void Dispose() {
                _httpClient?.Dispose();
            }
        }
    }
}