using System;
using System.Security.Cryptography;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class SigningSettingsTests {
        private readonly SigningSettings _sut;

        public SigningSettingsTests() {
            _sut = new SigningSettings {
                Expires = TimeSpan.FromMinutes(5),
                KeyId = new KeyId("client1"),
                SignatureAlgorithm = new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA384),
                Headers = new[] {
                    HeaderName.PredefinedHeaderNames.RequestTarget,
                    HeaderName.PredefinedHeaderNames.Date,
                    new HeaderName("dalion_app_id")
                },
                DigestHashAlgorithm = HashAlgorithmName.SHA256,
                AuthorizationScheme = "UnitTestAuth",
                EnableNonce = true
            };
        }

        public class Validate : SigningSettingsTests {
            [Fact]
            public void IsIValidatable() {
                Action act = () => ((IValidatable)_sut).Validate();
                act.Should().NotThrow();
            }
            
            [Fact]
            public void WhenKeyIdIsEmpty_ThrowsValidationException() {
                _sut.KeyId = KeyId.Empty;
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }

            [Fact]
            public void WhenNoSignatureAlgorithmIsSpecified_ThrowsValidationException() {
                _sut.SignatureAlgorithm = null;
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }

            [Fact]
            public void WhenHeadersIsNull_DoesNotThrow() {
                _sut.Headers = null;
                Action act = () => _sut.Validate();
                act.Should().NotThrow();
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
            public void WhenEverythingIsValid_DoesNotThrow() {
                Action act = () => _sut.Validate();
                act.Should().NotThrow();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyAuthorizationScheme_ThrowsValidationException(string nullOrEmpty) {
                _sut.AuthorizationScheme = nullOrEmpty;
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }
        }

        public class Dispose : SigningSettingsTests {
            public Dispose() {
                _sut.SignatureAlgorithm = A.Fake<ISignatureAlgorithm>();
            }

            [Fact]
            public void DisposesOfSignatureAlgorithm() {
                _sut.Dispose();

                A.CallTo(() => _sut.SignatureAlgorithm.Dispose())
                    .MustHaveHappened();
            }

            [Fact]
            public void WhenSignatureAlgorithmIsNull_DoesNotThrow() {
                _sut.SignatureAlgorithm = null;

                Action act = () => _sut.Dispose();
                
                act.Should().NotThrow();
            }
        }

        public class Clone : SigningSettingsTests {
            [Fact]
            public void ClonesAllPropertiesExceptEvents() {
                var actual = (SigningSettings)_sut.Clone();
                actual.Should().BeEquivalentTo(_sut, options => options.Excluding(_ => _.Events));
            }
        }
    }
}