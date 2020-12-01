using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning {
    public class SigningSettingsTests {
        private readonly SigningSettings _sut;

        public SigningSettingsTests() {
            _sut = new SigningSettings {
                Expires = TimeSpan.FromMinutes(9),
                KeyId = new KeyId("client1"),
                SignatureAlgorithm = new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA384),
                Headers = new[] {
                    HeaderName.PredefinedHeaderNames.RequestTarget,
                    HeaderName.PredefinedHeaderNames.Date,
                    new HeaderName("dalion_app_id")
                },
                DigestHashAlgorithm = HashAlgorithmName.SHA256,
                AuthorizationScheme = "UnitTestAuth",
                EnableNonce = false,
                AutomaticallyAddRecommendedHeaders = false,
                RequestTargetEscaping = RequestTargetEscaping.RFC2396,
                Events = new RequestSigningEvents {
                    OnRequestSigned = (message, signature, settings) => Task.CompletedTask,
                    OnRequestSigning = (message, settings) => Task.CompletedTask,
                    OnSigningStringComposed = (HttpRequestMessage requestToSign, ref string signingString) => Task.CompletedTask,
                    OnSignatureCreated = (message, signature, settings) => Task.CompletedTask
                },
                UseDeprecatedAlgorithmParameter = true
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
            public void WhenHeadersIsNull_AndRecommendedHeadersAreAutomaticallyAdded_DoesNotThrow() {
                _sut.Headers = null;
                _sut.AutomaticallyAddRecommendedHeaders = true;
                Action act = () => _sut.Validate();
                act.Should().NotThrow();
            }

            [Fact]
            public void WhenHeadersIsEmpty_AndRecommendedHeadersAreAutomaticallyAdded_DoesNotThrow() {
                _sut.Headers = Array.Empty<HeaderName>();
                _sut.AutomaticallyAddRecommendedHeaders = true;
                Action act = () => _sut.Validate();
                act.Should().NotThrow();
            }

            [Fact]
            public void WhenHeadersIsNull_AndRecommendedHeadersAreNotAutomaticallyAdded_ThrowsValidationException() {
                _sut.Headers = null;
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }

            [Fact]
            public void WhenHeadersIsEmpty_AndRecommendedHeadersAreNotAutomaticallyAdded_ThrowsValidationException() {
                _sut.Headers = Array.Empty<HeaderName>();
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
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

            [Theory]
            [InlineData("unknown")]
            [InlineData("unknown-sha256")]
            [InlineData("ec25519")]
            [InlineData("ec25519-sha256")]
            public void GivenUnsupportedSignatureAlgorithm_ThrowsValidationException(string unsupportedAlgorithm) {
                _sut.SignatureAlgorithm = new CustomSignatureAlgorithm(unsupportedAlgorithm);
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }

            [Fact]
            public void GivenUndefinedRequestTargetEscaping_ThrowsValidationException() {
                _sut.RequestTargetEscaping = (RequestTargetEscaping) (-99);
                Action act = () => _sut.Validate();
                act.Should().Throw<ValidationException>();
            }
        }

        public class GetValidationErrors : SigningSettingsTests {
            [Fact]
            public void IsIValidatable() {
                Action act = () => ((IValidatable)_sut).GetValidationErrors();
                act.Should().NotThrow();
            }
            
            [Fact]
            public void WhenKeyIdIsEmpty_IsInvalid() {
                _sut.KeyId = KeyId.Empty;
                var actual = _sut.GetValidationErrors().ToList();
                actual.Should().NotBeNullOrEmpty();
                actual.Should().Contain(_ => _.PropertyName == nameof(_sut.KeyId));
            }

            [Fact]
            public void WhenNoSignatureAlgorithmIsSpecified_IsInvalid() {
                _sut.SignatureAlgorithm = null;
                var actual = _sut.GetValidationErrors().ToList();
                actual.Should().NotBeNullOrEmpty();
                actual.Should().Contain(_ => _.PropertyName == nameof(_sut.SignatureAlgorithm));
            }

            [Fact]
            public void WhenHeadersIsNull_AndRecommendedHeadersAreNotAutomaticallyAdded_IsInvalid() {
                _sut.Headers = null;
                var actual = _sut.GetValidationErrors().ToList();
                actual.Should().NotBeNullOrEmpty();
                actual.Should().Contain(_ => _.PropertyName == nameof(_sut.Headers));
            }

            [Fact]
            public void WhenHeadersIsEmpty_AndRecommendedHeadersAreNotAutomaticallyAdded_IsInvalid() {
                _sut.Headers = Array.Empty<HeaderName>();
                var actual = _sut.GetValidationErrors().ToList();
                actual.Should().NotBeNullOrEmpty();
                actual.Should().Contain(_ => _.PropertyName == nameof(_sut.Headers));
            }

            [Fact]
            public void WhenHeadersIsNull_AndRecommendedHeadersAreAutomaticallyAdded_IsValid() {
                _sut.Headers = null;
                _sut.AutomaticallyAddRecommendedHeaders = true;
                var actual = _sut.GetValidationErrors().ToList();
                actual.Should().NotBeNull().And.BeEmpty();
            }

            [Fact]
            public void WhenHeadersIsEmpty_AndRecommendedHeadersAreAutomaticallyAdded_IsValid() {
                _sut.Headers = Array.Empty<HeaderName>();
                _sut.AutomaticallyAddRecommendedHeaders = true;
                var actual = _sut.GetValidationErrors().ToList();
                actual.Should().NotBeNull().And.BeEmpty();
            }

            [Fact]
            public void WhenExpiresIsNegative_IsInvalid() {
                _sut.Expires = TimeSpan.FromSeconds(-1);
                var actual = _sut.GetValidationErrors().ToList();
                actual.Should().NotBeNullOrEmpty();
                actual.Should().Contain(_ => _.PropertyName == nameof(_sut.Expires));
            }

            [Fact]
            public void WhenExpiresIsZero_IsInvalid() {
                _sut.Expires = TimeSpan.Zero;
                var actual = _sut.GetValidationErrors().ToList();
                actual.Should().NotBeNullOrEmpty();
                actual.Should().Contain(_ => _.PropertyName == nameof(_sut.Expires));
            }

            [Fact]
            public void WhenEverythingIsValid_IsValid() {
                var actual = _sut.GetValidationErrors().ToList();
                actual.Should().NotBeNull().And.BeEmpty();
            }
            
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void GivenNullOrEmptyAuthorizationScheme_IsInvalid(string nullOrEmpty) {
                _sut.AuthorizationScheme = nullOrEmpty;
                var actual = _sut.GetValidationErrors().ToList();
                actual.Should().NotBeNullOrEmpty();
                actual.Should().Contain(_ => _.PropertyName == nameof(_sut.AuthorizationScheme));
            }

            [Theory]
            [InlineData("unknown")]
            [InlineData("unknown-sha256")]
            [InlineData("ec25519")]
            [InlineData("ec25519-sha256")]
            public void GivenUnsupportedSignatureAlgorithm_IsInvalid(string unsupportedAlgorithm) {
                _sut.SignatureAlgorithm = new CustomSignatureAlgorithm(unsupportedAlgorithm);
                var actual = _sut.GetValidationErrors().ToList();
                actual.Should().NotBeNullOrEmpty();
                actual.Should().Contain(_ => _.PropertyName == nameof(_sut.SignatureAlgorithm));
            }

            [Fact]
            public void GivenUndefinedRequestTargetEscaping_IsInvalid() {
                _sut.RequestTargetEscaping = (RequestTargetEscaping) (-99);
                var actual = _sut.GetValidationErrors().ToList();
                actual.Should().NotBeNullOrEmpty();
                actual.Should().Contain(_ => _.PropertyName == nameof(_sut.RequestTargetEscaping));
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
            public void ClonesAllSimpleProperties() {
                var actual = (SigningSettings)_sut.Clone();
                actual.Should().BeEquivalentTo(_sut, options => options.Excluding(_ => _.Events));
            }

            [Fact]
            public void ClonesEvents() {
                var actual = (SigningSettings)_sut.Clone();

                actual.Events.OnRequestSigned.Should().BeSameAs(_sut.Events.OnRequestSigned);
                actual.Events.OnRequestSigning.Should().BeSameAs(_sut.Events.OnRequestSigning);
                actual.Events.OnSigningStringComposed.Should().BeSameAs(_sut.Events.OnSigningStringComposed);
                actual.Events.OnSignatureCreated.Should().BeSameAs(_sut.Events.OnSignatureCreated);
            }
        }
    }
}