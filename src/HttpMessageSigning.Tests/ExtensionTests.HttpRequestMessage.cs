using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using Dalion.HttpMessageSigning.TestUtils;
using FluentAssertions;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public partial class ExtensionTests {
        public class ForHttpRequestMessage : ExtensionTests {
            public class ToHttpRequestForSigning : ForHttpRequestMessage {
                private readonly HttpRequestMessage _httpRequestMessage;

                public ToHttpRequestForSigning() {
                    _httpRequestMessage = new HttpRequestMessage {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri("https://dalion.eu:9000/tests/api/rsc1?query=1&cache=false"),
                        Headers = {{"H1", "v1"}, {"H2", new[] {"v2", "v3"}}, {"H3", ""}},
                        Content = new StringContent("abc123", Encoding.UTF8, MediaTypeNames.Text.Plain) {
                            Headers = {{"Category", "42"}}
                        }
                    };
                    _httpRequestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                }

                [Fact]
                public void GivenNullRequest_ReturnsNull() {
                    var actual = Extensions.ToHttpRequestForSigning(httpRequestMessage: null);
                    actual.Should().BeNull();
                }
                
                [Fact]
                public void GivenAbsoluteUri_CopiesUriPathAndQuery() {
                    var actual = _httpRequestMessage.ToHttpRequestForSigning();
                    var expectedUri = "/tests/api/rsc1?query=1&cache=false".ToUri();
                    actual.RequestUri.Should().Be(expectedUri);
                }
                
                [Fact]
                public void GivenRelativeUriAbsoluteUri_CopiesUriPathAndQuery() {
                    _httpRequestMessage.RequestUri = new Uri("/tests/api/rsc1?query=1&cache=false", UriKind.Relative);
                    var actual = _httpRequestMessage.ToHttpRequestForSigning();
                    var expectedUri = "/tests/api/rsc1?query=1&cache=false".ToUri();
                    actual.RequestUri.Should().Be(expectedUri);
                }
                
                [Fact]
                public void GivenAbsoluteUri_RFC3986EscapesPathAndQueryString() {
                    _httpRequestMessage.RequestUri = new Uri("https://dalion.eu:9000/tests/api/{Brooks} was here/create/David & Partners + Siebe at 100% * co.?query+string={brooks}");
                    var actual = _httpRequestMessage.ToHttpRequestForSigning();
                    var expectedUri = "/tests/api/%7BBrooks%7D%20was%20here/create/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%2Bstring=%7Bbrooks%7D".ToUri();
                    actual.RequestUri.Should().Be(expectedUri);
                }
                
                [Fact]
                public void GivenRelativeUri_RFC3986EscapesPathAndQueryString() {
                    _httpRequestMessage.RequestUri = new Uri("/tests/api/{Brooks} was here/create/David & Partners + Siebe at 100% * co.?query+string={brooks}", UriKind.Relative);
                    var actual = _httpRequestMessage.ToHttpRequestForSigning();
                    var expectedUri = "/tests/api/%7BBrooks%7D%20was%20here/create/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%2Bstring=%7Bbrooks%7D".ToUri();
                    actual.RequestUri.Should().Be(expectedUri);
                }
                
                [Fact]
                public void GivenAbsoluteUri_RFC3986EscapesPartiallyEscapedPathAndQueryString() {
                    _httpRequestMessage.RequestUri = new Uri("https://dalion.eu:9000/tests/api/{Brooks} was here/create/David%20&%20Partners%20+%20Siebe%20at%20100%25%20*%20co.?query+string={brooks}");
                    var actual = _httpRequestMessage.ToHttpRequestForSigning();
                    var expectedUri = "/tests/api/%7BBrooks%7D%20was%20here/create/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%2Bstring=%7Bbrooks%7D".ToUri();
                    actual.RequestUri.Should().Be(expectedUri);
                }
                
                [Fact]
                public void GivenRelativeUri_RFC3986EscapesRFC2396EscapedPathAndQueryString() {
                    _httpRequestMessage.RequestUri = new Uri("/tests/api/%7BBrooks%7D%20was%20here/create/David%20&%20Partners%20+%20Siebe%20at%20100%25%20*%20co.?query+string={brooks}", UriKind.Relative);
                    var actual = _httpRequestMessage.ToHttpRequestForSigning();
                    var expectedUri = "/tests/api/%7BBrooks%7D%20was%20here/create/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%2Bstring=%7Bbrooks%7D".ToUri();
                    actual.RequestUri.Should().Be(expectedUri);
                }
                
                [Fact]
                public void GivenAbsoluteUri_RFC3986EscapesRFC2396EscapedPathAndQueryString() {
                    _httpRequestMessage.RequestUri = new Uri("https://dalion.eu:9000/tests/api/%7BBrooks%7D%20was%20here/create/David%20&%20Partners%20+%20Siebe%20at%20100%25%20*%20co.?query+string={brooks}");
                    var actual = _httpRequestMessage.ToHttpRequestForSigning();
                    var expectedUri = "/tests/api/%7BBrooks%7D%20was%20here/create/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%2Bstring=%7Bbrooks%7D".ToUri();
                    actual.RequestUri.Should().Be(expectedUri);
                }
                
                [Fact]
                public void GivenRelativeUri_RFC3986EscapesPartiallyEscapedPathAndQueryString() {
                    _httpRequestMessage.RequestUri = new Uri("/tests/api/{Brooks} was here/create/David%20&%20Partners%20+%20Siebe%20at%20100%25%20*%20co.?query+string={brooks}", UriKind.Relative);
                    var actual = _httpRequestMessage.ToHttpRequestForSigning();
                    var expectedUri = "/tests/api/%7BBrooks%7D%20was%20here/create/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%2Bstring=%7Bbrooks%7D".ToUri();
                    actual.RequestUri.Should().Be(expectedUri);
                }
                
                [Fact]
                public void GivenAbsoluteUri_KeepsRFC3686Escaping() {
                    _httpRequestMessage.RequestUri = new Uri("https://dalion.eu:9000/tests/api/%7BBrooks%7D%20was%20here/create/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%2Bstring=%7Bbrooks%7D");
                    var actual = _httpRequestMessage.ToHttpRequestForSigning();
                    var expectedUri = "/tests/api/%7BBrooks%7D%20was%20here/create/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%2Bstring=%7Bbrooks%7D".ToUri();
                    actual.RequestUri.Should().Be(expectedUri);
                }
                
                [Fact]
                public void GivenRelativeUri_KeepsRFC3986Escaping() {
                    _httpRequestMessage.RequestUri = new Uri("/tests/api/%7BBrooks%7D%20was%20here/create/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%2Bstring=%7Bbrooks%7D", UriKind.Relative);
                    var actual = _httpRequestMessage.ToHttpRequestForSigning();
                    var expectedUri = "/tests/api/%7BBrooks%7D%20was%20here/create/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%2Bstring=%7Bbrooks%7D".ToUri();
                    actual.RequestUri.Should().Be(expectedUri);
                }
                
                [Fact]
                public void GivenAbsoluteUri_OmitsHash() {
                    _httpRequestMessage.RequestUri = new Uri("http://dalion.eu/api/resource/id1?blah=true#section=one");

                    var actual = _httpRequestMessage.ToHttpRequestForSigning();
                    
                    var expectedUri = "/api/resource/id1?blah=true".ToUri();
                    actual.RequestUri.Should().Be(expectedUri);
                }
                
                [Fact]
                public void GivenRelativeUri_OmitsHash() {
                    _httpRequestMessage.RequestUri = new Uri("/api/resource/id1?blah=true#section=one", UriKind.Relative);

                    var actual = _httpRequestMessage.ToHttpRequestForSigning();
                    
                    var expectedUri = "/api/resource/id1?blah=true".ToUri();
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
                    _httpRequestMessage.Method = new HttpMethod(method);

                    var actual = _httpRequestMessage.ToHttpRequestForSigning();

                    actual.Method.Should().Be(new HttpMethod(method));
                }

                [Fact]
                public void CopiesHeaders() {
                    var actual = _httpRequestMessage.ToHttpRequestForSigning();

                    var expectedHeaders = new HeaderDictionary(new Dictionary<string, StringValues> {
                        {"H1", "v1"},
                        {"H2", new[] {"v2", "v3"}},
                        {"H3", ""},
                        {"Category", "42"},
                        {"Content-Type", "text/html"}
                    });
                    actual.Headers.Should().BeEquivalentTo(expectedHeaders);
                }

                [Fact]
                public void CopiesContentHeadersToHeaders() {
                    var actual = _httpRequestMessage.ToHttpRequestForSigning();

                    actual.Headers.Contains("Category").Should().BeTrue();
                    actual.Headers.Contains("Content-Type").Should().BeTrue();
                }
            }
        }
    }
}