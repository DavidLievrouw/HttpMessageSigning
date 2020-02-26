using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification {
    public class DefaultSignatureHeadersProviderTests {
        private readonly DefaultSignatureHeadersProvider _sut;

        public DefaultSignatureHeadersProviderTests() {
            _sut = new DefaultSignatureHeadersProvider();
        }

        public class ProvideDefaultHeaders : DefaultSignatureHeadersProviderTests {
            [Fact]
            public void WhenAlgorithmIsRSA_ReturnsExpectedHeaderNames() {
                var actual = _sut.ProvideDefaultHeaders(new CustomSignatureAlgorithm("RSA"));
                actual.Should().BeEquivalentTo(new[] {
                    HeaderName.PredefinedHeaderNames.RequestTarget,
                    HeaderName.PredefinedHeaderNames.Date
                });
            }
            
            [Fact]
            public void WhenAlgorithmIsRSA_IncorrectlyCased_ReturnsExpectedHeaderNames() {
                var actual = _sut.ProvideDefaultHeaders(new CustomSignatureAlgorithm("Rsa"));
                actual.Should().BeEquivalentTo(new[] {
                    HeaderName.PredefinedHeaderNames.RequestTarget,
                    HeaderName.PredefinedHeaderNames.Date
                });
            }
            
            [Fact]
            public void WhenAlgorithmIsHMAC_ReturnsExpectedHeaderNames() {
                var actual = _sut.ProvideDefaultHeaders(new CustomSignatureAlgorithm("HMAC"));
                actual.Should().BeEquivalentTo(new[] {
                    HeaderName.PredefinedHeaderNames.RequestTarget,
                    HeaderName.PredefinedHeaderNames.Date
                });
            }
            
            [Fact]
            public void WhenAlgorithmIsHMAC_IncorrectlyCased_ReturnsExpectedHeaderNames() {
                var actual = _sut.ProvideDefaultHeaders(new CustomSignatureAlgorithm("Hmac"));
                actual.Should().BeEquivalentTo(new[] {
                    HeaderName.PredefinedHeaderNames.RequestTarget,
                    HeaderName.PredefinedHeaderNames.Date
                });
            }
            
            [Fact]
            public void WhenAlgorithmIsECDSA_ReturnsExpectedHeaderNames() {
                var actual = _sut.ProvideDefaultHeaders(new CustomSignatureAlgorithm("ECDSA"));
                actual.Should().BeEquivalentTo(new[] {
                    HeaderName.PredefinedHeaderNames.RequestTarget,
                    HeaderName.PredefinedHeaderNames.Date
                });
            }
            
            [Fact]
            public void WhenAlgorithmIsECDSA_IncorrectlyCased_ReturnsExpectedHeaderNames() {
                var actual = _sut.ProvideDefaultHeaders(new CustomSignatureAlgorithm("Ecdsa"));
                actual.Should().BeEquivalentTo(new[] {
                    HeaderName.PredefinedHeaderNames.RequestTarget,
                    HeaderName.PredefinedHeaderNames.Date
                });
            }
            
            [Fact]
            public void WhenAlgorithmIsSomethingElse_ReturnsExpectedHeaderNames() {
                var actual = _sut.ProvideDefaultHeaders(new CustomSignatureAlgorithm("DALION"));
                actual.Should().BeEquivalentTo(new[] {
                    HeaderName.PredefinedHeaderNames.RequestTarget,
                    HeaderName.PredefinedHeaderNames.Created,
                    HeaderName.PredefinedHeaderNames.Expires
                });
            }
        }
    }
}