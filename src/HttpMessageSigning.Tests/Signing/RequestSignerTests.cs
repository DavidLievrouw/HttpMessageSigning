using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Dalion.HttpMessageSigning.Signing {
    public class RequestSignerTests {
        private readonly IAuthorizationHeaderParamCreator _authorizationHeaderParamCreator;
        private readonly ISignatureCreator _signatureCreator;
        private readonly SigningSettings _signingSettings;
        private readonly ISignatureHeaderEnsurer _dateHeaderEnsurer;
        private readonly ISignatureHeaderEnsurer _digestHeaderEnsurer;
        private readonly ISystemClock _systemClock;
        private readonly ILogger<RequestSigner> _logger;
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
                KeyId = new KeyId("client1"),
                SignatureAlgorithm = new HMACSignatureAlgorithm("s3cr3t", HashAlgorithmName.SHA512),
                Headers = new[] {
                    HeaderName.PredefinedHeaderNames.RequestTarget,
                    HeaderName.PredefinedHeaderNames.Date,
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
                _signingSettings.KeyId = KeyId.Empty; // Make invalid
                Func<Task> act = () => _sut.Sign(_httpRequest);
                act.Should().Throw<ValidationException>();
            }

            [Fact]
            public async Task SignsUsingSettingsThatCanBeModifiedByEvents() {
                _signingSettings.Events.OnRequestSigning = (message, settings) => {
                    settings.Expires = TimeSpan.FromHours(3);
                    return Task.CompletedTask;
                };

                Expression<Func<SigningSettings, bool>> modifiedSigningSettings = s => s.KeyId == _signingSettings.KeyId && s.Expires == TimeSpan.FromHours(3);
                
                var signature = new Signature {String = "abc123="};
                A.CallTo(() => _signatureCreator.CreateSignature(_httpRequest, A<SigningSettings>.That.Matches(modifiedSigningSettings), _timeOfSigning))
                    .Returns(signature);
                
                var authParam = "signature=abc123=";
                A.CallTo(() => _authorizationHeaderParamCreator.CreateParam(signature))
                    .Returns(authParam);
                
                await _sut.Sign(_httpRequest);
                
                _httpRequest.Headers.Authorization.Should().Be(new AuthenticationHeaderValue("SignedHttpRequest", "signature=abc123="));
            }
            
            [Fact]
            public async Task EnsuresDateHeader_BeforeSigning() {
                Expression<Func<SigningSettings, bool>> modifiedSigningSettings = s => s.KeyId == _signingSettings.KeyId && s.Expires == _signingSettings.Expires;
                
                var signature = new Signature {String = "abc123="};
                A.CallTo(() => _signatureCreator.CreateSignature(_httpRequest, A<SigningSettings>.That.Matches(modifiedSigningSettings), _timeOfSigning))
                    .Returns(signature);

                await _sut.Sign(_httpRequest);

                A.CallTo(() => _dateHeaderEnsurer.EnsureHeader(_httpRequest, A<SigningSettings>.That.Matches(modifiedSigningSettings), _timeOfSigning)).MustHaveHappened()
                    .Then(A.CallTo(() => _signatureCreator.CreateSignature(_httpRequest, A<SigningSettings>.That.Matches(modifiedSigningSettings), _timeOfSigning)).MustHaveHappened())
                    .Then(A.CallTo(() => _authorizationHeaderParamCreator.CreateParam(signature)).MustHaveHappened());
            }

            [Fact]
            public async Task EnsuresDigestHeader_BeforeSigning() {
                Expression<Func<SigningSettings, bool>> modifiedSigningSettings = s => s.KeyId == _signingSettings.KeyId && s.Expires == _signingSettings.Expires;
                
                var signature = new Signature {String = "abc123="};
                A.CallTo(() => _signatureCreator.CreateSignature(_httpRequest, A<SigningSettings>.That.Matches(modifiedSigningSettings), _timeOfSigning))
                    .Returns(signature);

                await _sut.Sign(_httpRequest);

                A.CallTo(() => _digestHeaderEnsurer.EnsureHeader(_httpRequest, A<SigningSettings>.That.Matches(modifiedSigningSettings), _timeOfSigning)).MustHaveHappened()
                    .Then(A.CallTo(() => _signatureCreator.CreateSignature(_httpRequest, A<SigningSettings>.That.Matches(modifiedSigningSettings), _timeOfSigning)).MustHaveHappened())
                    .Then(A.CallTo(() => _authorizationHeaderParamCreator.CreateParam(signature)).MustHaveHappened());
            }

            [Fact]
            public async Task SetsExpectedAuthorizationHeaderInRequest() {
                Expression<Func<SigningSettings, bool>> modifiedSigningSettings = s => s.KeyId == _signingSettings.KeyId && s.Expires == _signingSettings.Expires;
                
                var signature = new Signature {String = "abc123="};
                A.CallTo(() => _signatureCreator.CreateSignature(_httpRequest, A<SigningSettings>.That.Matches(modifiedSigningSettings), _timeOfSigning))
                    .Returns(signature);

                var authParam = "signature=abc123=";
                A.CallTo(() => _authorizationHeaderParamCreator.CreateParam(signature))
                    .Returns(authParam);

                await _sut.Sign(_httpRequest);

                _httpRequest.Headers.Authorization.Should().Be(new AuthenticationHeaderValue("SignedHttpRequest", "signature=abc123="));
            }

            [Fact]
            public async Task OverwritesAuthorizationHeaderValueInRequest() {
                _httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Custom", "john.doe");

                Expression<Func<SigningSettings, bool>> modifiedSigningSettings = s => s.KeyId == _signingSettings.KeyId && s.Expires == _signingSettings.Expires;
                
                var signature = new Signature {String = "abc123="};
                A.CallTo(() => _signatureCreator.CreateSignature(_httpRequest, A<SigningSettings>.That.Matches(modifiedSigningSettings), _timeOfSigning))
                    .Returns(signature);

                var authParam = "signature=abc123=";
                A.CallTo(() => _authorizationHeaderParamCreator.CreateParam(signature))
                    .Returns(authParam);

                await _sut.Sign(_httpRequest);

                _httpRequest.Headers.Authorization.Should().Be(new AuthenticationHeaderValue("SignedHttpRequest", "signature=abc123="));
            }

            [Fact]
            public async Task BeforeSigning_InvokesEvent_WhenNotNull() {
                var onRequestSigning = A.Fake<Func<HttpRequestMessage, SigningSettings, Task>>();
                _signingSettings.Events.OnRequestSigning = onRequestSigning;
                
                await _sut.Sign(_httpRequest);

                A.CallTo(onRequestSigning).MustHaveHappened()
                    .Then(A.CallTo(() => _signatureCreator.CreateSignature(A<HttpRequestMessage>._, A<SigningSettings>._, A<DateTimeOffset>._)).MustHaveHappened());
            }
            
            [Fact]
            public async Task AfterSigning_InvokesEvent_WhenNotNull() {
                var onRequestSigned = A.Fake<Func<HttpRequestMessage, Signature, SigningSettings, Task>>();
                _signingSettings.Events.OnRequestSigned = onRequestSigned;
                
                await _sut.Sign(_httpRequest);

                A.CallTo(() => _signatureCreator.CreateSignature(A<HttpRequestMessage>._, A<SigningSettings>._, A<DateTimeOffset>._)).MustHaveHappened()
                    .Then(A.CallTo(onRequestSigned).MustHaveHappened());
            }

            [Fact]
            public void WhenEventsAreNull_DoesNotThrow() {
                _signingSettings.Events = null;
                
                Func<Task> act = () => _sut.Sign(_httpRequest);
                
                act.Should().NotThrow();
            }
        }

        public class Dispose : RequestSignerTests {
            private readonly ISignatureAlgorithm _signatureAlgorithm;

            public Dispose() {
                _signatureAlgorithm = A.Fake<ISignatureAlgorithm>();
                _signingSettings.SignatureAlgorithm = _signatureAlgorithm;
            }

            [Fact]
            public void DisposesOfSignatureAlgorithm() {
                _sut.Dispose();
                
                A.CallTo(() => _signatureAlgorithm.Dispose())
                    .MustHaveHappened();
            }
            
            [Fact]
            public void WhenSignatureAlgorithmIsNull_DoesNotThrow() {
                _signingSettings.SignatureAlgorithm = null;

                Action act = () => _sut.Dispose();
                
                act.Should().NotThrow();
            }
        }
    }
}