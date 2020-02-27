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
    public partial class ExtensionsTests {
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
                        QueryString = new QueryString("?query=1&cache=false"),
                        Headers = {
                            {HeaderName.PredefinedHeaderNames.Digest, "SHA-256=xyz123="}
                        }
                    };
                    _client = new Client("client", new CustomSignatureAlgorithm("Custom"));
                    _signature = new Signature {Headers = new[] {HeaderName.PredefinedHeaderNames.Digest}};
                }

                [Fact]
                public void GivenNullInput_ReturnsNull() {
                    Microsoft.AspNetCore.Http.HttpRequest nullRequest = null;
                    // ReSharper disable once ExpressionIsAlwaysNull
                    var actual = nullRequest.ToRequestForSigning(_client.SignatureAlgorithm, _signature);
                    actual.Should().BeNull();
                }

                [Fact]
                public void GivenNullSignatureAlgorithm_ThrowsArgumentNullException() {
                    Action act = () => _httpRequest.ToRequestForSigning(null, _signature);
                    act.Should().Throw<ArgumentNullException>();
                }

                [Fact]
                public void GivenNullSignature_ThrowsArgumentNullException() {
                    Action act = () => _httpRequest.ToRequestForSigning(_client.SignatureAlgorithm, null);
                    act.Should().Throw<ArgumentNullException>();
                }

                [Fact]
                public void AllowsNullMethod() {
                    _httpRequest.Method = null;

                    var actual = _httpRequest.ToRequestForSigning(_client.SignatureAlgorithm, _signature);

                    actual.Method.Should().Be(HttpMethod.Get);
                }

                [Fact]
                public void CopiesUri() {
                    var actual = _httpRequest.ToRequestForSigning(_client.SignatureAlgorithm, _signature);
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

                    var actual = _httpRequest.ToRequestForSigning(_client.SignatureAlgorithm, _signature);

                    actual.Method.Should().Be(new HttpMethod(method));
                }

                [Fact]
                public void CopiesHeaders() {
                    _httpRequest.Headers.Add("dalion-empty-header", string.Empty);
                    _httpRequest.Headers.Add("dalion-single-header", "one");
                    _httpRequest.Headers.Add("dalion-multi-header", new StringValues(new[] {"one", "2"}));

                    var actual = _httpRequest.ToRequestForSigning(_client.SignatureAlgorithm, _signature);

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
                public void ReadsContentTypeAsHeader() {
                    _httpRequest.Headers.Add("dalion-empty-header", string.Empty);
                    _httpRequest.Headers.Add("dalion-single-header", "one");
                    _httpRequest.Headers.Add("dalion-multi-header", new StringValues(new[] {"one", "2"}));
                    _httpRequest.ContentType = "application/json";

                    var actual = _httpRequest.ToRequestForSigning(_client.SignatureAlgorithm, _signature);

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
                public void SetsSignatureAlgorithm() {
                    var actual = _httpRequest.ToRequestForSigning(_client.SignatureAlgorithm, _signature);

                    actual.SignatureAlgorithmName.Should().Be("Custom");
                }

                [Fact]
                public void WhenThereIsNoBody_SetsBodyToNull() {
                    _httpRequest.Body = null;

                    var actual = _httpRequest.ToRequestForSigning(_client.SignatureAlgorithm, _signature);

                    actual.Body.Should().BeNull();
                }

                [Fact]
                public void WhenThereIsBody_AndDigestHeaderIsIncludedInSignature_ButDigestHeaderIsNotInRequest_ReadsBody() {
                    var bodyPayload = "This is the body payload";
                    _httpRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyPayload));
                    _httpRequest.ContentType = "text/plain";

                    _httpRequest.Headers.Remove(HeaderName.PredefinedHeaderNames.Digest);
                    
                    var actual = _httpRequest.ToRequestForSigning(_client.SignatureAlgorithm, _signature);

                    _httpRequest.Body.Should().NotBe(actual.Body); // Should not be the original stream, but a copy of it
                    actual.Body.Should().Be(bodyPayload);
                }

                [Fact]
                public void WhenThereIsBody_AndDigestHeaderIsNotIncludedInSignature_ButDigestHeaderIsInRequest_ReadsBody() {
                    var bodyPayload = "This is the body payload";
                    _httpRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyPayload));
                    _httpRequest.ContentType = "text/plain";

                    _signature.Headers = Array.Empty<HeaderName>();
                    
                    var actual = _httpRequest.ToRequestForSigning(_client.SignatureAlgorithm, _signature);

                    _httpRequest.Body.Should().NotBe(actual.Body); // Should not be the original stream, but a copy of it

                    actual.Body.Should().NotBeNull();
                    actual.Body.Should().Be(bodyPayload);
                }

                [Fact]
                public void WhenThereIsBody_AndDigestHeaderIsPresentInRequest_AndDigestHeaderIsPartOfTheSignature_ReadsBody() {
                    var bodyPayload = "This is the body payload";
                    _httpRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyPayload));
                    _httpRequest.ContentType = "text/plain";

                    var actual = _httpRequest.ToRequestForSigning(_client.SignatureAlgorithm, _signature);

                    _httpRequest.Body.Should().NotBe(actual.Body); // Should not be the original stream, but a copy of it

                    actual.Body.Should().NotBeNull();
                    actual.Body.Should().Be(bodyPayload);
                }
                
                [Fact]
                public void WhenThereIsBody_ButDigestHeaderIsNotPresentAndNotIncludedInSignature_SetsBodyToNullAndDoesNotReadIt() {
                    var bodyPayload = "This is the body payload";
                    _httpRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyPayload));
                    _httpRequest.ContentType = "text/plain";

                    _httpRequest.Headers.Remove(HeaderName.PredefinedHeaderNames.Digest);
                    _signature.Headers = Array.Empty<HeaderName>();

                    var actual = _httpRequest.ToRequestForSigning(_client.SignatureAlgorithm, _signature);

                    actual.Body.Should().BeNull();
                    _httpRequest.Body.Position.Should().Be(0);
                }

                [Fact]
                public void SetsRequestBodyStreamBackToPositionZero() {
                    var bodyPayload = "This is the body payload";
                    _httpRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyPayload));
                    _httpRequest.ContentType = "text/plain";

                    _httpRequest.ToRequestForSigning(_client.SignatureAlgorithm, _signature);

                    _httpRequest.Body.Position.Should().Be(0);
                }
            }
        }
    }
}