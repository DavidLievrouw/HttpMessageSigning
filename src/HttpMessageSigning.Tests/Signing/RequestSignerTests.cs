using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Dalion.HttpMessageSigning.Logging;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Signing {
    public class RequestSignerTests {
        private readonly IAuthorizationHeaderParamCreator _authorizationHeaderParamCreator;
        private readonly ISignatureCreator _signatureCreator;
        private readonly SigningSettings _signingSettings;
        private readonly ISignatureHeaderEnsurer _dateHeaderEnsurer;
        private readonly ISignatureHeaderEnsurer _digestHeaderEnsurer;
        private readonly ISystemClock _systemClock;
        private readonly IHttpMessageSigningLogger<RequestSigner> _logger;
        private readonly RequestSigner _sut;

        public RequestSignerTests() {
            FakeFactory.Create(
                out _signatureCreator,
                out _authorizationHeaderParamCreator,
                out _dateHeaderEnsurer,
                out _digestHeaderEnsurer,
                out _systemClock,
                out _logger);
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
            _sut = new RequestSigner(
                _signatureCreator,
                _authorizationHeaderParamCreator,
                _signingSettings,
                _dateHeaderEnsurer,
                _digestHeaderEnsurer,
                _systemClock,
                _logger);
        }

        public class Sign : RequestSignerTests {
            private readonly HttpRequestMessage _httpRequest;
            private readonly DateTimeOffset _timeOfSigning;

            public Sign() {
                _httpRequest = new HttpRequestMessage {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("http://dalion.eu/api/resource/id1")
                };
                _timeOfSigning = new DateTimeOffset(2020, 2, 24, 11, 20, 14, TimeSpan.Zero);
                A.CallTo(() => _systemClock.UtcNow).Returns(_timeOfSigning);
            }

            [Fact]
            public void GivenNullRequest_ThrowsArgumentNullException() {
                Func<Task> act = () => _sut.Sign(null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void GivenInvalidSettings_ThrowsValidationException() {
                _signingSettings.ClientKey = null; // Make invalid
                Func<Task> act = () => _sut.Sign(_httpRequest);
                act.Should().Throw<ValidationException>();
            }

            [Fact]
            public async Task EnsuresDateHeader_BeforeSigning() {
                var signature = new Signature {String = "abc123="};
                A.CallTo(() => _signatureCreator.CreateSignature(_httpRequest, _signingSettings, _timeOfSigning))
                    .Returns(signature);

                await _sut.Sign(_httpRequest);

                A.CallTo(() => _dateHeaderEnsurer.EnsureHeader(_httpRequest, _signingSettings, _timeOfSigning)).MustHaveHappened()
                    .Then(A.CallTo(() => _signatureCreator.CreateSignature(_httpRequest, _signingSettings, _timeOfSigning)).MustHaveHappened())
                    .Then(A.CallTo(() => _authorizationHeaderParamCreator.CreateParam(signature)).MustHaveHappened());
            }

            [Fact]
            public async Task EnsuresDigestHeader_BeforeSigning() {
                var signature = new Signature {String = "abc123="};
                A.CallTo(() => _signatureCreator.CreateSignature(_httpRequest, _signingSettings, _timeOfSigning))
                    .Returns(signature);

                await _sut.Sign(_httpRequest);

                A.CallTo(() => _digestHeaderEnsurer.EnsureHeader(_httpRequest, _signingSettings, _timeOfSigning)).MustHaveHappened()
                    .Then(A.CallTo(() => _signatureCreator.CreateSignature(_httpRequest, _signingSettings, _timeOfSigning)).MustHaveHappened())
                    .Then(A.CallTo(() => _authorizationHeaderParamCreator.CreateParam(signature)).MustHaveHappened());
            }

            [Fact]
            public async Task SetsExpectedAuthorizationHeaderInRequest() {
                var signature = new Signature {String = "abc123="};
                A.CallTo(() => _signatureCreator.CreateSignature(_httpRequest, _signingSettings, _timeOfSigning))
                    .Returns(signature);

                var authParam = "signature=abc123=";
                A.CallTo(() => _authorizationHeaderParamCreator.CreateParam(signature))
                    .Returns(authParam);

                await _sut.Sign(_httpRequest);

                _httpRequest.Headers.Authorization.Should().Be(new AuthenticationHeaderValue("Signature", "signature=abc123="));
            }

            [Fact]
            public async Task OverwritesAuthorizationHeaderValueInRequest() {
                _httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Custom", "john.doe");

                var signature = new Signature {String = "abc123="};
                A.CallTo(() => _signatureCreator.CreateSignature(_httpRequest, _signingSettings, _timeOfSigning))
                    .Returns(signature);

                var authParam = "signature=abc123=";
                A.CallTo(() => _authorizationHeaderParamCreator.CreateParam(signature))
                    .Returns(authParam);

                await _sut.Sign(_httpRequest);

                _httpRequest.Headers.Authorization.Should().Be(new AuthenticationHeaderValue("Signature", "signature=abc123="));
            }
        }
    }
}