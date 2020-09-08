using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Dalion.HttpMessageSigning.Signing {
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
                [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
                public void GivenNullRequest_ReturnsNull() {
                    HttpRequestMessage nullRequest = null;
                    var actual = nullRequest.ToHttpRequestForSigning();
                    actual.Should().BeNull();
                }
                
                [Fact]
                public void GivenAbsoluteUri_CopiesUri() {
                    var actual = _httpRequestMessage.ToHttpRequestForSigning();
                    actual.RequestUri.Should().Be(_httpRequestMessage.RequestUri);
                }
                
                [Fact]
                public void GivenRelativeUriAbsoluteUri_CopiesUri() {
                    _httpRequestMessage.RequestUri = new Uri("/tests/api/rsc1?query=1&cache=false", UriKind.Relative);
                    var actual = _httpRequestMessage.ToHttpRequestForSigning();
                    actual.RequestUri.Should().Be(_httpRequestMessage.RequestUri);
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