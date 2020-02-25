using System;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class SigningSettingsTests {
        private readonly SigningSettings _sut;

        public SigningSettingsTests() {
            _sut = new SigningSettings {
                Expires = TimeSpan.FromMinutes(5),
                ClientKey = new ClientKey {
                    Id = new KeyId("client1"),
                    Secret = new HMACSecret("s3cr3t")
                },
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
                _sut.ClientKey = null;
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }
            
            [Fact]
            public void WhenNoHashAlgorithmIsSelected_ThrowsValidationException() {
                _sut.HashAlgorithm = HashAlgorithm.None;
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }
            
            [Fact]
            public void WhenHeadersIsNull_ThrowsValidationException() {
                _sut.Headers = null;
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
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
                act.Should().Throw<ValidationException>();
            }
            
            [Fact]
            public void WhenExpiresIsZero_ThrowsValidationException() {
                _sut.Expires = TimeSpan.Zero;
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }
            
            [Fact]
            public void WhenClientKeyIsNull_ThrowsValidationException() {
                _sut.ClientKey = null;
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }
            
            [Fact]
            public void WhenClientKeyIsInvalid_ThrowsValidationException() {
                _sut.ClientKey.Id = KeyId.Empty; // Make invalid
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }
            
            [Fact]
            public void WhenEverythingIsValid_DoesNotThrow() {
                Action act = () => _sut.Validate();
                act.Should().NotThrow();
            }
        }
    }
}