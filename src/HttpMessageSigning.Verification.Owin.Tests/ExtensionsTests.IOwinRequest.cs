using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using FluentAssertions;
using Microsoft.Extensions.Primitives;
using Microsoft.Owin;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.Owin {
    public class ExtensionsTests {
        public class ForIOwinRequest : ExtensionsTests {
            public class ToHttpRequestForSigning : ForIOwinRequest {
                private readonly IOwinRequest _owinRequest;

                public ToHttpRequestForSigning() {
                    _owinRequest = new FakeOwinRequest {
                        Method = "GET",
                        Scheme = "https",
                        Host = new HostString("unittest.com:9000")
                    };
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
                    var actual = _owinRequest.ToHttpRequestForSigning();
                    actual.Method.Should().Be(new HttpMethod(method));
                }

                [Fact]
                public void CopiesUri() {
                    _owinRequest.Scheme = "https";
                    _owinRequest.Host = new HostString("unittest.com:9000");
                    _owinRequest.PathBase = new PathString("/api");
                    _owinRequest.Path = new PathString("/policies/test");
                    
                    var actual = _owinRequest.ToHttpRequestForSigning();
                    
                    actual.RequestUri.Should().Be("/api/policies/test");
                }

                [Fact]
                public void CopiesAllHeadersWithValues() {
                    _owinRequest.Headers.Add("simple-value", new[] {"v1"});
                    _owinRequest.Headers.Add("multiple-value", new[] {"v1", "v2"});
                    _owinRequest.Headers.Add("no-value", Array.Empty<string>());

                    var actual = _owinRequest.ToHttpRequestForSigning();

                    actual.Headers.ToDictionary().Should().BeEquivalentTo(new Dictionary<string, StringValues> {
                        {"simple-value", (StringValues)new[] {"v1"}},
                        {"multiple-value", (StringValues)new[] {"v1", "v2"}},
                        {"no-value", StringValues.Empty}
                    });
                }

                [Fact]
                public void WhenThereAreNoHeaders_SetsEmptyHeaders() {
                    _owinRequest.Headers.Clear();

                    var actual = _owinRequest.ToHttpRequestForSigning();

                    actual.Headers.ToDictionary().Should().NotBeNull().And.BeEmpty();
                }

                [Fact]
                public void WhenThereIsADigestHeader_CopiesBodyAndHeader() {
                    var bodyString = "abc";
                    var bodyBytes = System.Text.Encoding.UTF8.GetBytes(bodyString);
                    
                    using (var bodyStream = new MemoryStream(bodyBytes)) {
                        _owinRequest.Headers.Add("simple-value", new[] {"v1"});
                        _owinRequest.Headers.Add("digest", new[] {"SHA-256=xyz123="});
                        _owinRequest.Body = bodyStream;

                        var actual = _owinRequest.ToHttpRequestForSigning();

                        actual.Headers.Should().Contain(_ => _.Key == "digest" && _.Value == "SHA-256=xyz123=");
                        actual.Body.Should().BeEquivalentTo(bodyBytes, options => options.WithStrictOrdering());
                    }
                }

                [Fact]
                public void WhenThereIsNoBody_DoesNotCopyBody() {
                    _owinRequest.Headers.Add("simple-value", new[] {"v1"});
                    _owinRequest.Headers.Add("digest", new[] {"SHA-256=xyz123="});
                    _owinRequest.Body = null;

                    var actual = _owinRequest.ToHttpRequestForSigning();

                    actual.Body.Should().BeNull();
                }

                [Fact]
                public void WhenThereIsNoDigestHeader_DoesNotCopyBody() {                   
                    var bodyString = "abc";
                    var bodyBytes = System.Text.Encoding.UTF8.GetBytes(bodyString);
                    
                    using (var bodyStream = new MemoryStream(bodyBytes)) {
                        _owinRequest.Headers.Add("simple-value", new[] {"v1"});
                        _owinRequest.Body = bodyStream;

                        var actual = _owinRequest.ToHttpRequestForSigning();

                        actual.Body.Should().BeNull();
                    }
                }

                [Fact]
                public void WhenBodyIsCopied_TheBodyStreamOfTheOriginalRequestShouldBeAtZero() {             
                    var bodyString = "abc";
                    var bodyBytes = System.Text.Encoding.UTF8.GetBytes(bodyString);
                    
                    using (var bodyStream = new MemoryStream(bodyBytes)) {
                        _owinRequest.Headers.Add("simple-value", new[] {"v1"});
                        _owinRequest.Headers.Add("digest", new[] {"SHA-256=xyz123="});
                        _owinRequest.Body = bodyStream;

                        _owinRequest.ToHttpRequestForSigning();

                        _owinRequest.Body.Position.Should().Be(0);
                    }
                }

                [Fact]
                public void WhenBodyWasCopied_ShouldHaveDisposedTheOriginalStream() {             
                    var bodyString = "abc";
                    var bodyBytes = System.Text.Encoding.UTF8.GetBytes(bodyString);
                    
                    using (var bodyStream = new MemoryStream(bodyBytes)) {
                        _owinRequest.Headers.Add("simple-value", new[] {"v1"});
                        _owinRequest.Headers.Add("digest", new[] {"SHA-256=xyz123="});
                        _owinRequest.Body = bodyStream;

                        _owinRequest.ToHttpRequestForSigning();

                        Action act = () => bodyStream.ReadByte();
                        act.Should().Throw<ObjectDisposedException>();
                    }
                }

                [Fact]
                public void WhenBodyWasNotCopied_DoesNotDisposeOriginalStream() {             
                    var bodyString = "abc";
                    var bodyBytes = System.Text.Encoding.UTF8.GetBytes(bodyString);
                    
                    using (var bodyStream = new MemoryStream(bodyBytes)) {
                        _owinRequest.Headers.Add("simple-value", new[] {"v1"});
                        _owinRequest.Body = bodyStream;

                        _owinRequest.ToHttpRequestForSigning();

                        Action act = () => bodyStream.ReadByte();
                        act.Should().NotThrow();
                    }
                }
            }
        }
    }
}