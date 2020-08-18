using System.Net.Http;
using FluentAssertions;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification {
    public class HttpRequestForVerificationTests {
        private readonly HttpRequestForVerification _sut;

        public HttpRequestForVerificationTests() {
            _sut = new HttpRequestForVerification {
                Headers = new HeaderDictionary {
                    {"h1", new StringValues(new[] {"v1", "v2"})},
                    {"h2", new StringValues(new[] {"v3"})},
                    {"h3", StringValues.Empty}
                },
                Method = HttpMethod.Options,
                RequestUri = "https://unittest.com:9000/api?test=true",
                Signature = new Signature {
                    KeyId = new KeyId("abc123"),
                    Algorithm = "hs2019",
                    String = "xyz001"
                }
            };
        }

        public class Clone : HttpRequestForVerificationTests {
            [Fact]
            public void ReturnsNewInstanceWithExpectedValues() {
                var actual = _sut.Clone();
                actual.Should().NotBe(_sut);
                actual.Should().BeEquivalentTo(_sut);
            }
        }
    }
}