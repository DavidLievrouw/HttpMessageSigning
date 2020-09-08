using System.Net.Http;
using Dalion.HttpMessageSigning.TestUtils;
using FluentAssertions;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Dalion.HttpMessageSigning {
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
                actual.Should().BeEquivalentTo(_sut);
            }
        }
    }
}