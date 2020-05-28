#if NETCORE
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.AspNetCore {
    public partial class ExtensionsTests {
        public class HttpRequest : ExtensionTests {
            public class ToRequestForSigning : HttpRequest {
                private readonly Microsoft.AspNetCore.Http.HttpRequest _httpRequest;
                private readonly Signature _signature;

                public ToRequestForSigning() {
                    _httpRequest = new DefaultHttpContext().Request;
                    _httpRequest.Method = "POST";
                    _httpRequest.Scheme = "https";
                    _httpRequest.Host = new HostString("dalion.eu", 9000);
                    _httpRequest.PathBase = new PathString("/tests");
                    _httpRequest.Path = new PathString("/api/rsc1");
                    _httpRequest.QueryString = new QueryString("?query=1&cache=false");
                    _httpRequest.Headers[HeaderName.PredefinedHeaderNames.Digest] = "SHA-256=xyz123=";
                    _signature = new Signature {Headers = new[] {HeaderName.PredefinedHeaderNames.Digest}};
                }

                [Fact]
                public async Task GivenNullInput_ReturnsNull() {
                    Microsoft.AspNetCore.Http.HttpRequest nullRequest = null;
                    // ReSharper disable once ExpressionIsAlwaysNull
                    var actual = await nullRequest.ToRequestForSigning(_signature);
                    actual.Should().BeNull();
                }

                [Fact]
                public void GivenNullSignature_ThrowsArgumentNullException() {
                    Func<Task> act = () => _httpRequest.ToRequestForSigning(null);
                    act.Should().Throw<ArgumentNullException>();
                }

                [Fact]
                public async Task AllowsNullMethod() {
                    _httpRequest.Method = null;

                    var actual = await _httpRequest.ToRequestForSigning(_signature);

                    actual.Method.Should().Be(HttpMethod.Get);
                }

                [Fact]
                public async Task CopiesUriPath() {
                    var actual = await _httpRequest.ToRequestForSigning(_signature);
                    var expectedUri = "/tests/api/rsc1";
                    actual.RequestUri.Should().Be(expectedUri);
                }               
                
                [Fact]
                public async Task CanHandleEmptyPathBase() {
                    _httpRequest.PathBase = PathString.Empty;
                    
                    var actual = await _httpRequest.ToRequestForSigning(_signature);
                    
                    var expectedUri = "/api/rsc1";
                    actual.RequestUri.Should().Be(expectedUri);
                }
                
                [Fact]
                public async Task DecodesUriPath() {
                    _httpRequest.PathBase = new PathString("/api");
                    _httpRequest.Path = new PathString("/{Brooks} was here/create/David%20%26%20Partners%20%2B%20Siebe%20at%20100%25%20%2A%20co.");
                    
                    var actual = await _httpRequest.ToRequestForSigning(_signature);
                    
                    var expectedUri = "/api/{Brooks} was here/create/David & Partners + Siebe at 100% * co.";
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

                    var actual = await _httpRequest.ToRequestForSigning(_signature);

                    actual.Method.Should().Be(new HttpMethod(method));
                }

                [Fact]
                public async Task CopiesHeaders() {
                    _httpRequest.Headers.Add("dalion-empty-header", string.Empty);
                    _httpRequest.Headers.Add("dalion-single-header", "one");
                    _httpRequest.Headers.Add("dalion-multi-header", new Microsoft.Extensions.Primitives.StringValues(new[] {"one", "2"}));

                    var actual = await _httpRequest.ToRequestForSigning(_signature);

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

                    var actual = await _httpRequest.ToRequestForSigning(_signature);

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
                public async Task WhenThereIsNoBody_SetsBodyToNull() {
                    _httpRequest.Body = null;

                    var actual = await _httpRequest.ToRequestForSigning(_signature);

                    actual.Body.Should().BeNull();
                }

                [Fact]
                public async Task ThereIsBody_AndDigestHeaderIsIncludedInSignature_ButDigestHeaderIsNotInRequest_ReadsBody() {
                    var bodyPayload = "This is the body payload";
                    var bodyBytes = Encoding.UTF8.GetBytes(bodyPayload);
                    _httpRequest.Body = new MemoryStream(bodyBytes);
                    _httpRequest.ContentType = "text/plain";

                    _httpRequest.Headers.Remove(HeaderName.PredefinedHeaderNames.Digest);
                    
                    var actual = await _httpRequest.ToRequestForSigning(_signature);

                    _httpRequest.Body.Should().NotBe(actual.Body); // Should not be the original stream, but a copy of it
                    actual.Body.Should().BeEquivalentTo(bodyBytes, options => options.WithStrictOrdering());
                }

                [Fact]
                public async Task WhenThereIsBody_AndDigestHeaderIsNotIncludedInSignature_ButDigestHeaderIsInRequest_ReadsBody() {
                    var bodyPayload = "This is the body payload";
                    var bodyBytes = Encoding.UTF8.GetBytes(bodyPayload);
                    _httpRequest.Body = new MemoryStream(bodyBytes);
                    _httpRequest.ContentType = "text/plain";

                    _signature.Headers = Array.Empty<HeaderName>();
                    
                    var actual = await _httpRequest.ToRequestForSigning(_signature);

                    _httpRequest.Body.Should().NotBe(actual.Body); // Should not be the original stream, but a copy of it

                    actual.Body.Should().NotBeNull();
                    actual.Body.Should().BeEquivalentTo(bodyBytes, options => options.WithStrictOrdering());
                }

                [Fact]
                public async Task WhenThereIsBody_AndDigestHeaderIsPresentInRequest_AndDigestHeaderIsPartOfTheSignature_ReadsBody() {
                    var bodyPayload = "This is the body payload";
                    var bodyBytes = Encoding.UTF8.GetBytes(bodyPayload);
                    _httpRequest.Body = new MemoryStream(bodyBytes);
                    _httpRequest.ContentType = "text/plain";

                    var actual = await _httpRequest.ToRequestForSigning(_signature);

                    _httpRequest.Body.Should().NotBe(actual.Body); // Should not be the original stream, but a copy of it

                    actual.Body.Should().NotBeNull();
                    actual.Body.Should().BeEquivalentTo(bodyBytes, options => options.WithStrictOrdering());
                }
                
                [Fact]
                public async Task WhenThereIsBody_ButDigestHeaderIsNotPresentAndNotIncludedInSignature_SetsBodyToNullAndDoesNotReadIt() {
                    var bodyPayload = "This is the body payload";
                    _httpRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyPayload));
                    _httpRequest.ContentType = "text/plain";

                    _httpRequest.Headers.Remove(HeaderName.PredefinedHeaderNames.Digest);
                    _signature.Headers = Array.Empty<HeaderName>();

                    var actual = await _httpRequest.ToRequestForSigning(_signature);

                    actual.Body.Should().BeNull();
                    _httpRequest.Body.Position.Should().Be(0);
                }

                [Fact]
                public async Task SetsRequestBodyStreamBackToPositionZero() {
                    var bodyPayload = "This is the body payload";
                    _httpRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyPayload));
                    _httpRequest.ContentType = "text/plain";

                    await _httpRequest.ToRequestForSigning(_signature);

                    _httpRequest.Body.Position.Should().Be(0);
                }
            }
        }
    }
}
#endif