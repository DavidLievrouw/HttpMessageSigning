using System.Net.Http;
using Dalion.HttpMessageSigning.TestUtils;
using FluentAssertions;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Dalion.HttpMessageSigning.Signing {
    public class HttpRequestForSigningTests {
        private readonly HttpRequestForSigning _sut;

        public HttpRequestForSigningTests() {
            _sut = new HttpRequestForSigning {
                Headers = new HeaderDictionary {
                    {"h1", new StringValues(new[] {"v1", "v2"})},
                    {"h2", new StringValues(new[] {"v3"})},
                    {"h3", StringValues.Empty}
                },
                Method = HttpMethod.Options,
                RequestUri = "https://unittest.com:9000/api?test=true".ToUri()
            };
        }

        public class Clone : HttpRequestForSigningTests {
            [Fact]
            public void ReturnsNewInstanceWithExpectedValues() {
                var actual = _sut.Clone();
                actual.Should().NotBe(_sut);
                actual.As<HttpRequestForSigning>().Should().BeEquivalentTo(_sut);
            }

            [Fact]
            public void GivenNullHeaders_SetsHeadersToNull() {
                _sut.Headers = null;

                var actual = _sut.Clone();

                actual.As<HttpRequestForSigning>().Headers.Should().BeNull();
            }
        }

        public class ToHttpRequestForSignatureString : HttpRequestForSigningTests {
            [Fact]
            public void CopiesMethod() {
                var actual = _sut.ToHttpRequestForSignatureString();

                actual.Method.Should().Be(_sut.Method);
            }

            [Fact]
            public void CopiesUri() {
                var actual = _sut.ToHttpRequestForSignatureString();

                actual.RequestUri.Should().Be(_sut.RequestUri);
            }

            [Fact]
            public void GivenNullHeaders_SetsHeadersToNull() {
                _sut.Headers = null;

                var actual = _sut.ToHttpRequestForSignatureString();

                actual.Headers.Should().BeNull();
            }

            [Fact]
            public void GivenHeaders_CopiesHeaders() {
                var actual = _sut.ToHttpRequestForSignatureString();

                actual.Headers.Should().BeEquivalentTo(_sut.Headers, options => options.WithStrictOrdering());
            }
        }
    }
}