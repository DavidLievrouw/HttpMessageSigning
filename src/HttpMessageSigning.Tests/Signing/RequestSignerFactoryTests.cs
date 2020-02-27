using System;
using System.Security.Cryptography;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Dalion.HttpMessageSigning.Signing {
    public class RequestSignerFactoryTests {
        private readonly IAuthorizationHeaderParamCreator _authorizationHeaderParamCreator;
        private readonly ILogger<RequestSigner> _logger;
        private readonly ISignatureCreator _signatureCreator;
        private readonly ISignatureHeaderEnsurer _dateHeaderEnsurer;
        private readonly ISignatureHeaderEnsurer _digestHeaderEnsurer;
        private readonly ISystemClock _systemClock;
        private readonly IRegisteredSignerSettingsStore _registeredSignerSettingsStore;
        private readonly RequestSignerFactory _sut;

        public RequestSignerFactoryTests() {
            FakeFactory.Create(
                out _signatureCreator,
                out _authorizationHeaderParamCreator,
                out _dateHeaderEnsurer,
                out _digestHeaderEnsurer,
                out _systemClock,
                out _logger,
                out _registeredSignerSettingsStore);
            _sut = new RequestSignerFactory(
                _signatureCreator,
                _authorizationHeaderParamCreator,
                _dateHeaderEnsurer,
                _digestHeaderEnsurer,
                _systemClock,
                _logger,
                _registeredSignerSettingsStore);
        }

        public class Create : RequestSignerFactoryTests {
            private readonly SigningSettings _signingSettings;
            private readonly KeyId _keyId;

            public Create() {
                _keyId = new KeyId("client1");
                _signingSettings = new SigningSettings {
                    Expires = TimeSpan.FromMinutes(5),
                    SignatureAlgorithm = new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA512),
                    Headers = new[] {
                        HeaderName.PredefinedHeaderNames.RequestTarget,
                        HeaderName.PredefinedHeaderNames.Date,
                        new HeaderName("dalion_app_id")
                    }
                };
            }

            [Fact]
            public void GivenNullSettings_ThrowsArgumentNullException() {
                Action act = () => _sut.Create(_keyId, null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenEmptyKeyId_ThrowsArgumentException() {
                Action act = () => _sut.Create(KeyId.Empty, _signingSettings);
                act.Should().Throw<ArgumentException>();
            }

            [Fact]
            public void GivenInvalidSettings_ThrowsValidationException() {
                _signingSettings.SignatureAlgorithm = null; // Make invalid
                Action act = () => _sut.Create(_keyId, _signingSettings);
                act.Should().Throw<ValidationException>();
            }

            [Fact]
            public void CreatesNewInstanceOfExpectedType() {
                var actual = _sut.Create(_keyId, _signingSettings);
                actual.Should().NotBeNull().And.BeAssignableTo<RequestSigner>();
            }
        }

        public class CreateFor : RequestSignerFactoryTests {
            private readonly KeyId _keyId;

            public CreateFor() {
                _keyId = new KeyId("client1");
            }

            [Fact]
            public void WhenKeyIdIsEmpty_ThrowsArgumentException() {
                Action act = () => _sut.CreateFor(KeyId.Empty);
                act.Should().Throw<ArgumentException>();
            }

            [Fact]
            public void WhenClientIsNotRegistered_ThrowsInvalidOperationException() {
                A.CallTo(() => _registeredSignerSettingsStore.Get(_keyId))
                    .Returns(null);

                Action act = () => _sut.CreateFor(_keyId);

                act.Should().Throw<InvalidOperationException>();
            }

            [Fact]
            public void CreatesNewInstanceOfExpectedType() {
                var signingSettings = new SigningSettings {
                    Expires = TimeSpan.FromMinutes(5),
                    SignatureAlgorithm = new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA512),
                    Headers = new[] {
                        HeaderName.PredefinedHeaderNames.RequestTarget,
                        HeaderName.PredefinedHeaderNames.Date,
                        new HeaderName("dalion_app_id")
                    }
                };
                A.CallTo(() => _registeredSignerSettingsStore.Get(_keyId))
                    .Returns(signingSettings);

                var actual = _sut.CreateFor(_keyId);

                actual.Should().NotBeNull().And.BeAssignableTo<RequestSigner>();
            }
        }
    }
}