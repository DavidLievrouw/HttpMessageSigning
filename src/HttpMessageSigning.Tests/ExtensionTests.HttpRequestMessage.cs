using System;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public partial class ExtensionTests {
        public class ForHttpRequestMessage : ExtensionTests {
            public class ToRequestForSigning : ForHttpRequestMessage {
                private readonly HttpRequestMessage _httpRequestMessage;

                public ToRequestForSigning() {
                    _httpRequestMessage = new HttpRequestMessage {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri("https://dalion.eu:9000/tests/api/rsc1?query=1&cache=false"),
                        Headers = {{"H1", "v1"}, {"H2", new[] {"v2", "v3"}}, {"H3", ""}},
                        Content = new StringContent("abc123", Encoding.UTF8, MediaTypeNames.Text.Plain) {
                            Headers = {{"Category", "42"}}
                        }
                    };
                }

                [Fact]
                public void WhenRequestMessageIsNull_ThrowsArgumentNullException() {
                    Action act = () => Extensions.ToRequestForSigning(null);
                    act.Should().Throw<ArgumentNullException>();
                }

                [Fact]
                public void CopiesUri() {
                    var actual = _httpRequestMessage.ToRequestForSigning();
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
                    _httpRequestMessage.Method = new HttpMethod(method);

                    var actual = _httpRequestMessage.ToRequestForSigning();

                    actual.Method.Should().Be(new HttpMethod(method));
                }

                [Fact]
                public void CopiesHeaders() {
                    var actual = _httpRequestMessage.ToRequestForSigning();

                    var expectedHeaders = new HeaderDictionary {
                        {"H1", "v1"},
                        {"H2", new[] {"v2", "v3"}},
                        {"H3", ""},
                        {"Category", "42"}
                    };
                    actual.Headers.Should().BeEquivalentTo(expectedHeaders);
                }

                [Fact]
                public void CopiesContentHeadersToHeaders() {
                    var actual = _httpRequestMessage.ToRequestForSigning();

                    actual.Headers.Should().Contain(_ => _.Key == "Category");
                }
            }
        }
    }
}