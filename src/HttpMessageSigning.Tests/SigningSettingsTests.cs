using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class SigningSettingsTests {
        private readonly SigningSettings _sut;

        public SigningSettingsTests() {
            _sut = new SigningSettings {
                Expires = TimeSpan.FromMinutes(5),
                KeyId = new SelfContainedKeyId(SignatureAlgorithm.HMAC, HashAlgorithm.SHA256, "abc123"),
                Headers = new[] {
                    HeaderName.PredefinedHeaderNames.RequestTarget,
                    HeaderName.PredefinedHeaderNames.Date,
                    HeaderName.PredefinedHeaderNames.Expires,
                    new HeaderName("dalion_app_id")
                },
                HashAlgorithm = HashAlgorithm.SHA384,
                SignatureAlgorithm = SignatureAlgorithm.RSA,
                DigestHashAlgorithm = HashAlgorithm.None
            };
        }

        public class Validate : SigningSettingsTests {
            [Fact]
            public void WhenKeyIdIsNull_ThrowsValidationException() {
                _sut.KeyId = null;
                Action act = () => _sut.Validate();
                act.Should().Throw<HttpMessageSigningValidationException>();
            }
            
            [Fact]
            public void WhenNoHashAlgorithmIsSelected_ThrowsValidationException() {
                _sut.HashAlgorithm = HashAlgorithm.None;
                Action act = () => _sut.Validate();
                act.Should().Throw<HttpMessageSigningValidationException>();
            }
            
            [Fact]
            public void WhenHeadersIsNull_ThrowsValidationException() {
                _sut.Headers = null;
                Action act = () => _sut.Validate();
                act.Should().Throw<HttpMessageSigningValidationException>();
            }
            
            [Fact]
            public void WhenHeadersIsEmpty_DoesNotThrow() {
                _sut.Headers = Array.Empty<HeaderName>();
                Action act = () => _sut.Validate();
                act.Should().NotThrow();
            }
            
            [Fact]
            public void WhenExpiresIsNegative_ThrowsValidationException() {
                _sut.Expires = TimeSpan.FromSeconds(-1);
                Action act = () => _sut.Validate();
                act.Should().Throw<HttpMessageSigningValidationException>();
            }
            
            [Fact]
            public void WhenExpiresIsZero_ThrowsValidationException() {
                _sut.Expires = TimeSpan.Zero;
                Action act = () => _sut.Validate();
                act.Should().Throw<HttpMessageSigningValidationException>();
            }
            
            [Fact]
            public void WhenEverythingIsValid_DoesNotThrow() {
                Action act = () => _sut.Validate();
                act.Should().NotThrow();
            }
        }
    }
}