using System;
using Dalion.HttpMessageSigning.Logging;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Signing {
    public class RequestSignerFactoryTests {
        private readonly IAuthorizationHeaderParamCreator _authorizationHeaderParamCreator;
        private readonly IHttpMessageSigningLogger<RequestSigner> _logger;
        private readonly ISignatureCreator _signatureCreator;
        private readonly RequestSignerFactory _sut;

        public RequestSignerFactoryTests() {
            FakeFactory.Create(out _signatureCreator, out _authorizationHeaderParamCreator, out _logger);
            _sut = new RequestSignerFactory(_signatureCreator, _authorizationHeaderParamCreator, _logger);
        }

        public class Create : RequestSignerFactoryTests {
            private readonly SigningSettings _signingSettings;

            public Create() {
                _signingSettings = new SigningSettings {
                    Expires = TimeSpan.FromMinutes(5),
                    ClientKey = new ClientKey {
                        Id = new KeyId("client1"),
                        Secret = new Secret("s3cr3t")
                    },
                    Headers = new[] {
                        HeaderName.PredefinedHeaderNames.RequestTarget,
                        HeaderName.PredefinedHeaderNames.Date,
                        HeaderName.PredefinedHeaderNames.Expires,
                        new HeaderName("dalion_app_id")
                    }
                };
            }

            [Fact]
            public void GivenNullSettings_ThrowsArgumentNullException() {
                Action act = () => _sut.Create(null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenInvalidSettings_ThrowsHttpMessageSigningValidationException() {
                _signingSettings.ClientKey = null; // Make invalid
                Action act = () => _sut.Create(_signingSettings);
                act.Should().Throw<HttpMessageSigningValidationException>();
            }

            [Fact]
            public void CreatesNewInstanceOfExpectedType() {
                var actual = _sut.Create(_signingSettings);
                actual.Should().NotBeNull().And.BeAssignableTo<RequestSigner>();
            }
        }
    }
}