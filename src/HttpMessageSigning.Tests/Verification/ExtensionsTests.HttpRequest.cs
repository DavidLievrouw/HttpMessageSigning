using System;
using System.Collections.Generic;
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
            public class ToRequestForSigning : HttpRequest {
                private readonly Microsoft.AspNetCore.Http.HttpRequest _httpRequest;
                private readonly Client _client;
                private readonly Signature _signature;

                public ToRequestForSigning() {
                    _httpRequest = new DefaultHttpRequest(new DefaultHttpContext()) {
                        Method = "POST",
                        Scheme = "https",
                        Host = new HostString("dalion.eu", 9000),
                        PathBase = new PathString("/tests"),
                        Path = new PathString("/api/rsc1"),
                        QueryString = new QueryString("?query=1&cache=false")
                    };
                    _client = new Client("client", new CustomSignatureAlgorithm("Custom"));
                    _signature = new Signature {
                        KeyId = _client.Id,
                        Created = new DateTimeOffset(2020, 2, 26, 15, 54, 22, TimeSpan.Zero),
                        Expires = new DateTimeOffset(2020, 2, 26, 15, 57, 22, TimeSpan.Zero)
                    };
                }

                [Fact]
                public void GivenNullInput_ReturnsNull() {
                    Microsoft.AspNetCore.Http.HttpRequest nullRequest = null;
                    // ReSharper disable once ExpressionIsAlwaysNull
                    var actual = nullRequest.ToRequestForSigning(_signature, _client);
                    actual.Should().BeNull();
                }
                
                [Fact]
                public void GivenNullSigningSignature_ThrowsArgumentNullException() {
                    Action act = () => _httpRequest.ToRequestForSigning(null, _client);
                    act.Should().Throw<ArgumentNullException>();
                }
                
                [Fact]
                public void GivenNullClient_ThrowsArgumentNullException() {
                    Action act = () => _httpRequest.ToRequestForSigning(_signature, null);
                    act.Should().Throw<ArgumentNullException>();
                }

                [Fact]
                public void AllowsNullMethod() {
                    _httpRequest.Method = null;

                    var actual = _httpRequest.ToRequestForSigning(_signature, _client);

                    actual.Method.Should().Be(HttpMethod.Get);
                }

                [Fact]
                public void CopiesUri() {
                    var actual = _httpRequest.ToRequestForSigning(_signature, _client);
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

                    var actual = _httpRequest.ToRequestForSigning(_signature, _client);

                    actual.Method.Should().Be(new HttpMethod(method));
                }

                [Fact]
                public void CopiesHeaders() {
                    _httpRequest.Headers.Add("dalion-empty-header", string.Empty);
                    _httpRequest.Headers.Add("dalion-single-header", "one");
                    _httpRequest.Headers.Add("dalion-multi-header", new StringValues(new[] {"one", "2"}));
                    _httpRequest.Headers.Add("Content-Type", "text/plain");
                    _httpRequest.ContentType = "application/json";
                    
                    var actual = _httpRequest.ToRequestForSigning(_signature, _client);

                    var expectedHeaders = new HeaderDictionary(new Dictionary<string, StringValues> {
                        {"dalion-empty-header", ""},
                        {"dalion-single-header", "one"},
                        {"dalion-multi-header", new StringValues(new[] {"one", "2"})},
                        {"Content-Type", "text/plain"},
                        {"Host", "dalion.eu:9000"},
                    });
                    actual.Headers.Should().BeEquivalentTo(expectedHeaders);
                }

                [Fact]
                public void SetsSignatureAlgorithm() {
                    var actual = _httpRequest.ToRequestForSigning(_signature, _client);

                    actual.SignatureAlgorithmName.Should().Be("Custom");
                }
            }
        }
    }
}