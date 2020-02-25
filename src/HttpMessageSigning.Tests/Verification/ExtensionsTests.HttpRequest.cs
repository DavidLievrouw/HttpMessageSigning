using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification {
    public class ExtensionsTests {
        public class HttpRequest : ExtensionTests {
            public class ToHttpRequestMessage : HttpRequest {
                private readonly Microsoft.AspNetCore.Http.HttpRequest _httpRequest;

                public ToHttpRequestMessage() {
                    _httpRequest = new DefaultHttpRequest(new DefaultHttpContext()) {
                        Method = "POST",
                        Scheme = "https",
                        Host = new HostString("dalion.eu", 9000),
                        PathBase = new PathString("/tests"),
                        Path = new PathString("/api/rsc1"),
                        QueryString = new QueryString("?query=1&cache=false")
                    };
                }

                [Fact]
                public void GivenNullInput_ReturnsNull() {
                    Microsoft.AspNetCore.Http.HttpRequest nullRequest = null;
                    // ReSharper disable once ExpressionIsAlwaysNull
                    var actual = nullRequest.ToHttpRequestMessage();
                    actual.Should().BeNull();
                }

                [Fact]
                public void CopiesUri() {
                    var actual = _httpRequest.ToHttpRequestMessage();
                    var expectedUri = new Uri("https://dalion.eu:9000/tests/api/rsc1?query=1&cache=false", UriKind.Absolute);
                    actual.RequestUri.Should().Be(expectedUri);
                }

                [Theory]
                [InlineData("POST")]
                [InlineData("PUT")]
                [InlineData("REPORT")]
                [InlineData("PATCH")]
                [InlineData("GET")]
                [InlineData("TRACE")]
                [InlineData("HEAD")]
                [InlineData("DELETE")]
                public void CopiesMethod(string method) {
                    _httpRequest.Method = method;

                    var actual = _httpRequest.ToHttpRequestMessage();

                    actual.Method.Should().Be(new System.Net.Http.HttpMethod(method));
                }

                [Theory]
                [InlineData("POST")]
                [InlineData("PUT")]
                [InlineData("REPORT")]
                [InlineData("PATCH")]
                public async Task WhenTheMethodSupportsBody_AndItHasBody_CopiesBody(string method) {
                    _httpRequest.Method = method;
                    var bodyBytes = Encoding.UTF8.GetBytes("abc123");
                    using (var bodyStream = new MemoryStream(bodyBytes)) {
                        _httpRequest.Body = bodyStream;

                        var actual = _httpRequest.ToHttpRequestMessage();

                        actual.Content.Should().BeAssignableTo<StreamContent>();
                        var actualBytes = await actual.Content.As<StreamContent>().ReadAsByteArrayAsync();
                        actualBytes.Should().BeEquivalentTo(bodyBytes);
                    }
                }

                [Theory]
                [InlineData("POST")]
                [InlineData("PUT")]
                [InlineData("REPORT")]
                [InlineData("PATCH")]
                public void WhenTheMethodSupportsBody_ButItDoesNotHaveBody_DoesNotSetBody(string method) {
                    _httpRequest.Method = method;
                    _httpRequest.Body = null;

                    var actual = _httpRequest.ToHttpRequestMessage();

                    actual.Content.Should().BeNull();
                }

                [Theory]
                [InlineData("GET")]
                [InlineData("TRACE")]
                [InlineData("HEAD")]
                [InlineData("DELETE")]
                public void WhenTheMethodDoesNotSupportBody_ButItHasABody_DoesNotCopyBody(string method) {
                    _httpRequest.Method = method;
                    var bodyBytes = Encoding.UTF8.GetBytes("abc123");
                    using (var bodyStream = new MemoryStream(bodyBytes)) {
                        _httpRequest.Body = bodyStream;

                        var actual = _httpRequest.ToHttpRequestMessage();

                        actual.Content.Should().BeNull();
                    }
                }

                [Theory]
                [InlineData("GET")]
                [InlineData("TRACE")]
                [InlineData("HEAD")]
                [InlineData("DELETE")]
                public void WhenTheMethodDoesNotSupportBody_AndItHasNoBody_DoesNotCopyBody(string method) {
                    _httpRequest.Method = method;
                    _httpRequest.Body = null;

                    var actual = _httpRequest.ToHttpRequestMessage();

                    actual.Content.Should().BeNull();
                }

                [Fact]
                public void CopiesHeadersAndSetsHostHeader() {
                    _httpRequest.Headers.Add("dalion-empty-header", string.Empty);
                    _httpRequest.Headers.Add("dalion-single-header", "one");
                    _httpRequest.Headers.Add("dalion-multi-header", new StringValues(new[] {"one", "2"}));

                    var actual = _httpRequest.ToHttpRequestMessage();

                    var expectedHeaders = new HeaderDictionary {
                        {"dalion-empty-header", ""},
                        {"dalion-single-header", "one"},
                        {"dalion-multi-header", new StringValues(new[] {"one", "2"})},
                        {"Host", "dalion.eu:9000"},
                    };
                    actual.Headers.Should().BeEquivalentTo(expectedHeaders);
                }

                [Fact]
                public void CopiesContentHeaders() {
                    _httpRequest.Method = "POST";
                    _httpRequest.Headers.Add("Content-Type", "application/json");

                    var bodyBytes = Encoding.UTF8.GetBytes("abc123");
                    using (var bodyStream = new MemoryStream(bodyBytes)) {
                        _httpRequest.Body = bodyStream;

                        var actual = _httpRequest.ToHttpRequestMessage();

                        var expectedHeaders = new HeaderDictionary {
                            {"Content-Type", "application/json"}
                        };
                        actual.Content.Headers.Should().BeEquivalentTo(expectedHeaders);
                    }
                }

                [Fact]
                public void WhenRequestHasNoContent_DoesNotCopyContentHeaders() {
                    _httpRequest.Method = "POST";
                    _httpRequest.Headers.Add("Content-Type", "application/json");
                    _httpRequest.Body = null;

                    var actual = _httpRequest.ToHttpRequestMessage();

                    actual.Content?.Headers.Should().BeNull();
                }
            }
        }
    }
}