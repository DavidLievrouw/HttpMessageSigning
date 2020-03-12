using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification {
    public partial class ExtensionsTests {
        public class HttpRequest : ExtensionTests {
            public class ToRequestForSigning : HttpRequest {
                private readonly Microsoft.AspNetCore.Http.HttpRequest _httpRequest;
                private readonly Client _client;

                public ToRequestForSigning() {
                    _httpRequest = new DefaultHttpContext().Request;
                    _httpRequest.Method = "POST";
                    _httpRequest.Scheme = "https";
                    _httpRequest.Host = new HostString("dalion.eu", 9000);
                    _httpRequest.PathBase = new PathString("/tests");
                    _httpRequest.Path = new PathString("/api/rsc1");
                    _httpRequest.QueryString = new QueryString("?query=1&cache=false");
                    _httpRequest.Headers[HeaderName.PredefinedHeaderNames.Digest] = "SHA-256=xyz123=";
                    _client = new Client("client", "Unit test app", new CustomSignatureAlgorithm("Custom"));
                }

                [Fact]
                public async Task GivenNullInput_ReturnsNull() {
                    Microsoft.AspNetCore.Http.HttpRequest nullRequest = null;
                    // ReSharper disable once ExpressionIsAlwaysNull
                    var actual = await nullRequest.ToRequestForSigning(_client.SignatureAlgorithm);
                    actual.Should().BeNull();
                }

                [Fact]
                public void GivenNullSignatureAlgorithm_ThrowsArgumentNullException() {
                    Func<Task> act = () => _httpRequest.ToRequestForSigning(null);
                    act.Should().Throw<ArgumentNullException>();
                }

                [Fact]
                public async Task AllowsNullMethod() {
                    _httpRequest.Method = null;

                    var actual = await _httpRequest.ToRequestForSigning(_client.SignatureAlgorithm);

                    actual.Method.Should().Be(HttpMethod.Get);
                }

                [Fact]
                public async Task CopiesUri() {
                    var actual = await _httpRequest.ToRequestForSigning(_client.SignatureAlgorithm);
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
                public async Task CopiesMethod(string method) {
                    _httpRequest.Method = method;

                    var actual = await _httpRequest.ToRequestForSigning(_client.SignatureAlgorithm);

                    actual.Method.Should().Be(new HttpMethod(method));
                }

                [Fact]
                public async Task CopiesHeaders() {
                    _httpRequest.Headers.Add("dalion-empty-header", string.Empty);
                    _httpRequest.Headers.Add("dalion-single-header", "one");
                    _httpRequest.Headers.Add("dalion-multi-header", new Microsoft.Extensions.Primitives.StringValues(new[] {"one", "2"}));

                    var actual = await _httpRequest.ToRequestForSigning(_client.SignatureAlgorithm);

                    var expectedHeaders = new HeaderDictionary(new Dictionary<string, StringValues> {
                        {"dalion-empty-header", ""},
                        {"dalion-single-header", "one"},
                        {"dalion-multi-header", new StringValues(new[] {"one", "2"})},
                        {"Host", "dalion.eu:9000"},
                        {HeaderName.PredefinedHeaderNames.Digest, "SHA-256=xyz123="}
                    });
                    actual.Headers.Should().BeEquivalentTo(expectedHeaders);
                }

                [Fact]
                public async Task ReadsContentTypeAsHeader() {
                    _httpRequest.Headers.Add("dalion-empty-header", string.Empty);
                    _httpRequest.Headers.Add("dalion-single-header", "one");
                    _httpRequest.Headers.Add("dalion-multi-header", new Microsoft.Extensions.Primitives.StringValues(new[] {"one", "2"}));
                    _httpRequest.ContentType = "application/json";

                    var actual = await _httpRequest.ToRequestForSigning(_client.SignatureAlgorithm);

                    var expectedHeaders = new HeaderDictionary(new Dictionary<string, StringValues> {
                        {"dalion-empty-header", ""},
                        {"dalion-single-header", "one"},
                        {"dalion-multi-header", new StringValues(new[] {"one", "2"})},
                        {"Content-Type", "application/json"},
                        {"Host", "dalion.eu:9000"},
                        {HeaderName.PredefinedHeaderNames.Digest, "SHA-256=xyz123="}
                    });
                    actual.Headers.Should().BeEquivalentTo(expectedHeaders);
                }

                [Fact]
                public async Task SetsSignatureAlgorithm() {
                    var actual = await _httpRequest.ToRequestForSigning(_client.SignatureAlgorithm);

                    actual.SignatureAlgorithmName.Should().Be("Custom");
                }

                [Fact]
                public async Task WhenThereIsNoBody_SetsBodyToNull() {
                    _httpRequest.Body = null;

                    var actual = await _httpRequest.ToRequestForSigning(_client.SignatureAlgorithm);

                    actual.Body.Should().BeNull();
                }

                [Fact]
                public async Task ThereIsBody_ButDigestHeaderIsNotInRequest_SetsBodyToNullAndDoesNotReadIt() {
                    var bodyPayload = "This is the body payload";
                    _httpRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyPayload));
                    _httpRequest.ContentType = "text/plain";

                    _httpRequest.Headers.Remove(HeaderName.PredefinedHeaderNames.Digest);
                    
                    var actual = await _httpRequest.ToRequestForSigning(_client.SignatureAlgorithm);

                    actual.Body.Should().BeNull();
                    _httpRequest.Body.Position.Should().Be(0);
                }

                [Fact]
                public async Task WhenThereIsBody_AndDigestHeaderIsInRequest_ReadsBody() {
                    var bodyPayload = "This is the body payload";
                    _httpRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyPayload));
                    _httpRequest.ContentType = "text/plain";
                    
                    var actual = await _httpRequest.ToRequestForSigning(_client.SignatureAlgorithm);

                    _httpRequest.Body.Should().NotBe(actual.Body); // Should not be the original stream, but a copy of it

                    actual.Body.Should().NotBeNull();
                    actual.Body.Should().Be(bodyPayload);
                }

                [Fact]
                public async Task SetsRequestBodyStreamBackToPositionZero() {
                    var bodyPayload = "This is the body payload";
                    _httpRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyPayload));
                    _httpRequest.ContentType = "text/plain";

                    await _httpRequest.ToRequestForSigning(_client.SignatureAlgorithm);

                    _httpRequest.Body.Position.Should().Be(0);
                }
            }
        }
    }
}