using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Primitives;
using Microsoft.Owin;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.Owin {
    public class ExtensionsTests {
        public class ForIOwinRequest : ExtensionsTests {
            public class ToHttpRequestForVerification : ForIOwinRequest {
                private readonly IOwinRequest _owinRequest;
                private readonly Signature _signature;

                public ToHttpRequestForVerification() {
                    _owinRequest = new OwinRequest {
                        Method = "GET",
                        Scheme = "https",
                        Host = new HostString("unittest.com:9000"),
                        PathBase = new PathString("/tests"),
                        Path = new PathString("/api/rsc1"),
                        QueryString = new QueryString("query=1&cache=false"),
                        Headers = {
                            {HeaderName.PredefinedHeaderNames.Digest, new [] {"SHA-256=xyz123="}}
                        }
                    };
                    _signature = new Signature {Headers = new[] {HeaderName.PredefinedHeaderNames.Digest}};
                }

                [Fact]
                public void GivenNullInput_ReturnsNull() {
                    IOwinRequest nullRequest = null;
                    // ReSharper disable once ExpressionIsAlwaysNull
                    var actual = nullRequest.ToHttpRequestForVerification(_signature);
                    actual.Should().BeNull();
                }

                [Fact]
                public void GivenNullSignature_ThrowsArgumentNullException() {
                    Action act = () => _owinRequest.ToHttpRequestForVerification(signature: null);
                    act.Should().Throw<ArgumentNullException>();
                }

                [Fact]
                public void CopiesSignature() { 
                    var actual = _owinRequest.ToHttpRequestForVerification(_signature);

                    actual.Signature.Should().Be(_signature);
                }
                
                [Theory]
                [InlineData("GET")]
                [InlineData("TRACE")]
                [InlineData("HEAD")]
                [InlineData("DELETE")]
                [InlineData("POST")]
                [InlineData("PUT")]
                [InlineData("REPORT")]
                [InlineData("PATCH")]
                public void CopiesMethod(string method) {
                    _owinRequest.Method = method;
                    var actual = _owinRequest.ToHttpRequestForVerification(_signature);
                    actual.Method.Should().Be(new HttpMethod(method));
                }

                [Fact]
                public void CopiesUriPath() {
                    _owinRequest.Scheme = "https";
                    _owinRequest.Host = new HostString("unittest.com:9000");
                    _owinRequest.PathBase = new PathString("/api");
                    _owinRequest.Path = new PathString("/policies/test");

                    var actual = _owinRequest.ToHttpRequestForVerification(_signature);

                    actual.RequestUri.Should().Be("/api/policies/test?query=1&cache=false");
                }

                [Fact]
                public void CanHandleEmptyPathBase() {
                    _owinRequest.Scheme = "https";
                    _owinRequest.Host = new HostString("unittest.com:9000");
                    _owinRequest.PathBase = PathString.Empty;
                    _owinRequest.Path = new PathString("/policies/test");

                    var actual = _owinRequest.ToHttpRequestForVerification(_signature);

                    actual.RequestUri.Should().Be("/policies/test?query=1&cache=false");
                }
          
                [Fact]
                public void CanHandleEmptyPath() {
                    _owinRequest.PathBase = PathString.Empty;
                    _owinRequest.Path = PathString.Empty;
                    
                    var actual = _owinRequest.ToHttpRequestForVerification(_signature);
                    
                    var expectedUri = "/?query=1&cache=false";
                    actual.RequestUri.Should().Be(expectedUri);
                }    
                
                [Fact]
                public void CanHandleEmptyPathAndQuery() {
                    _owinRequest.PathBase = PathString.Empty;
                    _owinRequest.Path = PathString.Empty;
                    _owinRequest.QueryString = QueryString.Empty;
                    
                    var actual = _owinRequest.ToHttpRequestForVerification(_signature);
                    
                    var expectedUri = "/";
                    actual.RequestUri.Should().Be(expectedUri);
                }
                
                [Fact]
                public void CanHandleDefaultPort() {
                    _owinRequest.Scheme = "https";
                    _owinRequest.Host = new HostString("unittest.com");
                    _owinRequest.PathBase = PathString.Empty;
                    _owinRequest.Path = new PathString("/policies/test");

                    var actual = _owinRequest.ToHttpRequestForVerification(_signature);

                    actual.RequestUri.Should().Be("/policies/test?query=1&cache=false");
                }

                [Fact]
                public void UrlEncodesRFC2396EscapedUriPathAndQueryString() {
                    _owinRequest.PathBase = new PathString("/api");
                    _owinRequest.Path = new PathString("/%7BBrooks%7D%20was%20here/create/David%20&%20Partners%20+%20Siebe%20at%20100%25%20*%20co.");
                    _owinRequest.QueryString = new QueryString("query+string=%7Bbrooks%7D");
                    
                    var actual = _owinRequest.ToHttpRequestForVerification(_signature);
                    
                    var expectedUri = "/api/%7BBrooks%7D%20was%20here/create/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%2Bstring=%7Bbrooks%7D";
                    actual.RequestUri.Should().Be(expectedUri);
                }
                
                [Fact]
                public void UrlEncodesRFC3986EscapedUriPathAndQueryString() {
                    _owinRequest.PathBase = new PathString("/api");
                    _owinRequest.Path = new PathString("/%7BBrooks%7D%20was%20here/create/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.");
                    _owinRequest.QueryString = new QueryString("query%2Bstring=%7Bbrooks%7D");
                    
                    var actual = _owinRequest.ToHttpRequestForVerification(_signature);
                    
                    var expectedUri = "/api/%7BBrooks%7D%20was%20here/create/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%2Bstring=%7Bbrooks%7D";
                    actual.RequestUri.Should().Be(expectedUri);
                }
                
                [Fact]
                public void UrlEncodesUnescapedUriPathAndQueryString() {
                    _owinRequest.PathBase = new PathString("/api");
                    _owinRequest.Path = new PathString("/{Brooks} was here/create/David & Partners + Siebe at 100% * co.");
                    _owinRequest.QueryString = new QueryString("query+string={brooks}");

                    var actual = _owinRequest.ToHttpRequestForVerification(_signature);

                    var expectedUri = "/api/%7BBrooks%7D%20was%20here/create/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.?query%2Bstring=%7Bbrooks%7D";
                    actual.RequestUri.Should().Be(expectedUri);
                }
                
                [Fact]
                public void CopiesAllHeadersWithValues() {
                    _owinRequest.Headers.Add("simple-value", new[] {"v1"});
                    _owinRequest.Headers.Add("multiple-value", new[] {"v1", "v2"});
                    _owinRequest.Headers.Add("no-value", Array.Empty<string>());

                    var actual = _owinRequest.ToHttpRequestForVerification(_signature);

                    actual.Headers.ToDictionary().Should().BeEquivalentTo(new Dictionary<string, StringValues> {
                        {"simple-value", "v1"},
                        {"multiple-value", (StringValues) new[] {"v1", "v2"}},
                        {"no-value", StringValues.Empty},
                        {HeaderName.PredefinedHeaderNames.Digest, "SHA-256=xyz123="},
                        {"Host", "unittest.com:9000"}
                    });
                }

                [Fact]
                public void WhenThereAreNoHeaders_SetsEmptyHeaders() {
                    _owinRequest.Headers.Clear();

                    var actual = _owinRequest.ToHttpRequestForVerification(_signature);

                    actual.Headers.ToDictionary().Should().NotBeNull().And.BeEmpty();
                }

                [Fact]
                public void WhenThereIsNoBody_SetsBodyToNull() {
                    _owinRequest.Body = null;

                    var actual = _owinRequest.ToHttpRequestForVerification(_signature);

                    actual.Body.Should().BeNull();
                }

                [Fact]
                public void WhenThereIsBody_AndDigestHeaderIsIncludedInSignature_ButDigestHeaderIsNotInRequest_ReadsBody() {
                    var bodyPayload = "This is the body payload";
                    var bodyBytes = Encoding.UTF8.GetBytes(bodyPayload);
                    _owinRequest.Body = new MemoryStream(bodyBytes);
                    _owinRequest.ContentType = "text/plain";

                    _owinRequest.Headers.Remove(HeaderName.PredefinedHeaderNames.Digest);
                    
                    var actual = _owinRequest.ToHttpRequestForVerification(_signature);

                    _owinRequest.Body.Should().NotBe(actual.Body); // Should not be the original stream, but a copy of it
                    actual.Body.Should().BeEquivalentTo(bodyBytes, options => options.WithStrictOrdering());
                }

                [Fact]
                public void WhenThereIsBody_AndDigestHeaderIsNotIncludedInSignature_ButDigestHeaderIsInRequest_ReadsBody() {
                    var bodyPayload = "This is the body payload";
                    var bodyBytes = Encoding.UTF8.GetBytes(bodyPayload);
                    _owinRequest.Body = new MemoryStream(bodyBytes);
                    _owinRequest.ContentType = "text/plain";

                    _signature.Headers = _signature.Headers.Where(h => h != HeaderName.PredefinedHeaderNames.Digest).ToArray();
                    
                    var actual = _owinRequest.ToHttpRequestForVerification(_signature);

                    _owinRequest.Body.Should().NotBe(actual.Body); // Should not be the original stream, but a copy of it

                    actual.Body.Should().NotBeNull();
                    actual.Body.Should().BeEquivalentTo(bodyBytes, options => options.WithStrictOrdering());
                }

                [Fact]
                public void WhenThereIsBody_AndDigestHeaderIsPresentInRequest_AndDigestHeaderIsPartOfTheSignature_ReadsBody() {
                    var bodyPayload = "This is the body payload";
                    var bodyBytes = Encoding.UTF8.GetBytes(bodyPayload);
                    _owinRequest.Body = new MemoryStream(bodyBytes);
                    _owinRequest.ContentType = "text/plain";

                    var actual = _owinRequest.ToHttpRequestForVerification(_signature);

                    _owinRequest.Body.Should().NotBe(actual.Body); // Should not be the original stream, but a copy of it

                    actual.Body.Should().NotBeNull();
                    actual.Body.Should().BeEquivalentTo(bodyBytes, options => options.WithStrictOrdering());
                }
                
                [Fact]
                public void WhenThereIsBody_ButDigestHeaderIsNotPresentAndNotIncludedInSignature_SetsBodyToNullAndDoesNotReadIt() {
                    var bodyPayload = "This is the body payload";
                    _owinRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyPayload));
                    _owinRequest.ContentType = "text/plain";

                    _owinRequest.Headers.Remove(HeaderName.PredefinedHeaderNames.Digest);
                    _signature.Headers = _signature.Headers.Where(h => h != HeaderName.PredefinedHeaderNames.Digest).ToArray();

                    var actual = _owinRequest.ToHttpRequestForVerification(_signature);

                    actual.Body.Should().BeNull();
                    _owinRequest.Body.Position.Should().Be(0);
                }

                [Fact]
                public void WhenBodyIsCopied_TheBodyStreamOfTheOriginalRequestShouldBeAtZero() {
                    var bodyString = "abc";
                    var bodyBytes = Encoding.UTF8.GetBytes(bodyString);

                    using (var bodyStream = new MemoryStream(bodyBytes)) {
                        _owinRequest.Headers.Add("simple-value", new[] {"v1"});
                        _owinRequest.Body = bodyStream;

                        _owinRequest.ToHttpRequestForVerification(_signature);

                        _owinRequest.Body.Position.Should().Be(0);
                    }
                }

                [Fact]
                public void WhenBodyWasCopied_ShouldHaveDisposedTheOriginalStream() {
                    var bodyString = "abc";
                    var bodyBytes = Encoding.UTF8.GetBytes(bodyString);

                    using (var bodyStream = new MemoryStream(bodyBytes)) {
                        _owinRequest.Headers.Add("simple-value", new[] {"v1"});
                        _owinRequest.Body = bodyStream;

                        _owinRequest.ToHttpRequestForVerification(_signature);

                        Action act = () => bodyStream.ReadByte();
                        act.Should().Throw<ObjectDisposedException>();
                    }
                }

                [Fact]
                public void WhenBodyWasNotCopied_DoesNotDisposeOriginalStream() {
                    var bodyString = "abc";
                    var bodyBytes = Encoding.UTF8.GetBytes(bodyString);

                    using (var bodyStream = new MemoryStream(bodyBytes)) {
                        _owinRequest.Headers.Add("simple-value", new[] {"v1"});
                        _owinRequest.Body = bodyStream;

                        _owinRequest.Headers.Remove(HeaderName.PredefinedHeaderNames.Digest);
                        _signature.Headers = _signature.Headers.Where(h => h != HeaderName.PredefinedHeaderNames.Digest).ToArray();
                        
                        _owinRequest.ToHttpRequestForVerification(_signature);

                        Action act = () => bodyStream.ReadByte();
                        act.Should().NotThrow();
                    }
                }
            }
        }
    }
}