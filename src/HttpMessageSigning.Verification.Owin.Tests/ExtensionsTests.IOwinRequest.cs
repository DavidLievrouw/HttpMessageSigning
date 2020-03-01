using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Microsoft.Extensions.Primitives;
using Microsoft.Owin;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.Owin {
    public class ExtensionsTests {
        public class ForIOwinRequest : ExtensionsTests {
            public class ToHttpRequest : ForIOwinRequest {
                private readonly IOwinRequest _owinRequest;

                public ToHttpRequest() {
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
                    var actual = _owinRequest.ToHttpRequest();
                    actual.Method.Should().Be(method);
                }

                [Theory]
                [InlineData("http")]
                [InlineData("https")]
                public void CopiesScheme(string scheme) {
                    _owinRequest.Scheme = scheme;
                    var actual = _owinRequest.ToHttpRequest();
                    actual.Scheme.Should().Be(scheme);
                }

                [Fact]
                public void CopiesHost() {
                    _owinRequest.Host = new HostString("unittest.com:9000");
                    var actual = _owinRequest.ToHttpRequest();
                    actual.Host.Should().Be(new Microsoft.AspNetCore.Http.HostString("unittest.com", 9000));
                }

                [Fact]
                public void CopiesPath() {
                    _owinRequest.Path = new PathString("/api/test");
                    var actual = _owinRequest.ToHttpRequest();
                    actual.Path.Should().Be(new Microsoft.AspNetCore.Http.PathString("/api/test"));
                }

                [Fact]
                public void WhenThereIsNoPath_SetsPathToNull() {
                    _owinRequest.Path = PathString.Empty;
                    var actual = _owinRequest.ToHttpRequest();
                    actual.Path.Should().Be(Microsoft.AspNetCore.Http.PathString.Empty);
                }

                [Fact]
                public void CopiesPathBase() {
                    _owinRequest.PathBase = new PathString("/policies");
                    var actual = _owinRequest.ToHttpRequest();
                    actual.PathBase.Should().Be(new Microsoft.AspNetCore.Http.PathString("/policies"));
                }

                [Fact]
                public void WhenThereIsNoPathBase_SetsPathToNull() {
                    _owinRequest.PathBase = PathString.Empty;
                    var actual = _owinRequest.ToHttpRequest();
                    actual.PathBase.Should().Be(Microsoft.AspNetCore.Http.PathString.Empty);
                }

                [Fact]
                public void CopiesAllHeadersWithValues() {
                    _owinRequest.Headers.Add("simple-value", new[] {"v1"});
                    _owinRequest.Headers.Add("multiple-value", new[] {"v1", "v2"});
                    _owinRequest.Headers.Add("no-value", Array.Empty<string>());

                    var actual = _owinRequest.ToHttpRequest();

                    actual.Headers.Should().BeEquivalentTo(new Microsoft.AspNetCore.Http.HeaderDictionary(new Dictionary<string, StringValues> {
                        {"simple-value", new[] {"v1"}},
                        {"multiple-value", new[] {"v1", "v2"}},
                        {"Host", new[] {"unittest.com:9000"}}
                    }));
                }

                [Fact]
                public void WhenThereAreNoHeaders_SetsOnlyHostHeader() {
                    _owinRequest.Headers.Clear();

                    var actual = _owinRequest.ToHttpRequest();

                    actual.Headers.Should().BeEquivalentTo(new Microsoft.AspNetCore.Http.HeaderDictionary(new Dictionary<string, StringValues> {
                        {"Host", new[] {"unittest.com:9000"}}
                    }));
                }

                [Fact]
                public void WhenThereIsADigestHeader_CopiesBodyAndHeader() {
                    using (var bodyStream = new MemoryStream(new byte[] {0x01, 0x02, 0x03})) {
                        _owinRequest.Headers.Add("simple-value", new[] {"v1"});
                        _owinRequest.Headers.Add("digest", new[] {"SHA-256=xyz123="});
                        _owinRequest.Body = bodyStream;

                        var actual = _owinRequest.ToHttpRequest();

                        actual.Headers.Should().Contain("digest", "SHA-256=xyz123=");
                        
                        using(var actualStream = new MemoryStream())
                        {
                            actual.Body.CopyTo(actualStream);
                            var actualBytes = actualStream.ToArray();
                            actualBytes.Should().BeEquivalentTo(new byte[] {0x01, 0x02, 0x03});
                        }
                    }
                }

                [Fact]
                public void WhenThereIsNoBody_DoesNotCopyBody() {
                    _owinRequest.Headers.Add("simple-value", new[] {"v1"});
                    _owinRequest.Headers.Add("digest", new[] {"SHA-256=xyz123="});
                    _owinRequest.Body = null;

                    var actual = _owinRequest.ToHttpRequest();

                    actual.Body.Should().Be(Stream.Null);
                }

                [Fact]
                public void WhenThereIsNoDigestHeader_DoesNotCopyBody() {
                    using (var bodyStream = new MemoryStream(new byte[] {0x01, 0x02, 0x03})) {
                        _owinRequest.Headers.Add("simple-value", new[] {"v1"});
                        _owinRequest.Body = bodyStream;

                        var actual = _owinRequest.ToHttpRequest();

                        actual.Body.Should().Be(Stream.Null);
                    }
                }

                [Fact]
                public void WhenBodyIsCopied_TheBodyStreamOfTheOriginalRequestShouldBeAtZero() {
                    using (var bodyStream = new MemoryStream(new byte[] {0x01, 0x02, 0x03})) {
                        _owinRequest.Headers.Add("simple-value", new[] {"v1"});
                        _owinRequest.Headers.Add("digest", new[] {"SHA-256=xyz123="});
                        _owinRequest.Body = bodyStream;

                        _owinRequest.ToHttpRequest();

                        _owinRequest.Body.Position.Should().Be(0);
                    }
                }

                [Fact]
                public void WhenBodyWasCopied_ShouldHaveDisposedTheOriginalStream() {
                    using (var bodyStream = new MemoryStream(new byte[] {0x01, 0x02, 0x03})) {
                        _owinRequest.Headers.Add("simple-value", new[] {"v1"});
                        _owinRequest.Headers.Add("digest", new[] {"SHA-256=xyz123="});
                        _owinRequest.Body = bodyStream;

                        _owinRequest.ToHttpRequest();

                        Action act = () => bodyStream.ReadByte();
                        act.Should().Throw<ObjectDisposedException>();
                    }
                }

                [Fact]
                public void WhenBodyWasNotCopied_DoesNotDisposeOriginalStream() {
                    using (var bodyStream = new MemoryStream(new byte[] {0x01, 0x02, 0x03})) {
                        _owinRequest.Headers.Add("simple-value", new[] {"v1"});
                        _owinRequest.Body = bodyStream;

                        _owinRequest.ToHttpRequest();

                        Action act = () => bodyStream.ReadByte();
                        act.Should().NotThrow();
                    }
                }
            }
        }
    }
}