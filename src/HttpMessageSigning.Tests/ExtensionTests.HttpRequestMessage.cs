using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public partial class ExtensionTests {
        public class ForHttpRequestMessage : ExtensionTests {
            public class ToRequestForSigning : ForHttpRequestMessage {
                private readonly HttpRequestMessage _httpRequestMessage;
                private readonly SigningSettings _signingSettings;

                public ToRequestForSigning() {
                    _httpRequestMessage = new HttpRequestMessage {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri("https://dalion.eu:9000/tests/api/rsc1?query=1&cache=false"),
                        Headers = {{"H1", "v1"}, {"H2", new[] {"v2", "v3"}}, {"H3", ""}},
                        Content = new StringContent("abc123", Encoding.UTF8, MediaTypeNames.Text.Plain) {
                            Headers = {{"Category", "42"}}
                        }
                    };
                    _httpRequestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                    _signingSettings = new SigningSettings {
                        Expires = TimeSpan.FromMinutes(5),
                        SignatureAlgorithm = new CustomSignatureAlgorithm("Custom")
                    };
                }

                [Fact]
                public void GivenNullRequest_ReturnsNull() {
                    var actual = Extensions.ToRequestForSigning(null, _signingSettings.SignatureAlgorithm);
                    actual.Should().BeNull();
                }
                
                [Fact]
                public void GivenNullSignatureAlgorithm_ThrowsArgumentNullException() {
                    Action act = () => _httpRequestMessage.ToRequestForSigning(null);
                    act.Should().Throw<ArgumentNullException>();
                }

                [Fact]
                public void CopiesUriPath() {
                    var actual = _httpRequestMessage.ToRequestForSigning( _signingSettings.SignatureAlgorithm);
                    var expectedUri = "/tests/api/rsc1";
                    actual.RequestUri.Should().Be(expectedUri);
                }
                
                [Fact]
                public void OmitsQueryString() {
                    _httpRequestMessage.RequestUri = new Uri("http://dalion.eu/api/resource/id1?blah=true");

                    var actual = _httpRequestMessage.ToRequestForSigning( _signingSettings.SignatureAlgorithm);
                    
                    var expectedUri = "/api/resource/id1";
                    actual.RequestUri.Should().Be(expectedUri);
                }

                [Fact]
                public void CanHandleRelativeUris() {
                    _httpRequestMessage.RequestUri = new Uri("/api/resource/id1", UriKind.Relative);

                    var actual = _httpRequestMessage.ToRequestForSigning( _signingSettings.SignatureAlgorithm);
                    
                    var expectedUri = "/api/resource/id1";
                    actual.RequestUri.Should().Be(expectedUri);
                }
                
                [Fact]
                public void CanHandleRelativeUrisWithQuery() {
                    _httpRequestMessage.RequestUri = new Uri("/api/resource/id1?blah=true", UriKind.Relative);

                    var actual = _httpRequestMessage.ToRequestForSigning( _signingSettings.SignatureAlgorithm);
                    
                    var expectedUri = "/api/resource/id1";
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

                    var actual = _httpRequestMessage.ToRequestForSigning( _signingSettings.SignatureAlgorithm);

                    actual.Method.Should().Be(new HttpMethod(method));
                }

                [Fact]
                public void CopiesHeaders() {
                    var actual = _httpRequestMessage.ToRequestForSigning( _signingSettings.SignatureAlgorithm);

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
                    var actual = _httpRequestMessage.ToRequestForSigning( _signingSettings.SignatureAlgorithm);

                    actual.Headers.Contains("Category").Should().BeTrue();
                    actual.Headers.Contains("Content-Type").Should().BeTrue();
                }
                
                [Fact]
                public void SetsSignatureAlgorithm() {
                    var actual = _httpRequestMessage.ToRequestForSigning( _signingSettings.SignatureAlgorithm);

                    actual.SignatureAlgorithmName.Should().Be("Custom");
                }
            }
        }
    }
}